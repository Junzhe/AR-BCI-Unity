using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class QRCodeSpawner : MonoBehaviour
{
    [Header("AR 管理器")]
    public ARTrackedImageManager trackedImageManager;

    [Header("交互物块 Prefab")]
    public GameObject targetPrefab;

    // 存储二维码名字 -> 生成物块
    private Dictionary<string, GameObject> spawnedTargets = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // 新识别到二维码
        foreach (var trackedImage in eventArgs.added)
        {
            string name = trackedImage.referenceImage.name.ToUpper();
            if (!spawnedTargets.ContainsKey(name))
            {
                // 实例化 prefab，作为二维码的子物体
                var go = Instantiate(targetPrefab, trackedImage.transform);
                go.name = $"Target-{name}";
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                spawnedTargets[name] = go;

                Debug.Log($"[QRCodeSpawner] Added target {name}");
            }
        }

        // 已经识别的二维码 → 更新位置 / 旋转
        foreach (var trackedImage in eventArgs.updated)
        {
            string name = trackedImage.referenceImage.name.ToUpper();
            if (spawnedTargets.ContainsKey(name))
            {
                var go = spawnedTargets[name];

                if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
                {
                    go.SetActive(true);
                    go.transform.position = trackedImage.transform.position;
                    go.transform.rotation = trackedImage.transform.rotation;
                }
                else
                {
                    // 丢失跟踪时隐藏
                    go.SetActive(false);
                }
            }
        }

        // 丢失的二维码 → 删除或隐藏物块
        foreach (var trackedImage in eventArgs.removed)
        {
            string name = trackedImage.referenceImage.name.ToUpper();
            if (spawnedTargets.ContainsKey(name))
            {
                var go = spawnedTargets[name];
                Destroy(go);
                spawnedTargets.Remove(name);

                Debug.Log($"[QRCodeSpawner] Removed target {name}");
            }
        }
    }
}
