using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using TMPro;

public class ImageTrackingManager : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public GameObject targetPrefab;

    private Dictionary<string, GameObject> spawnedTargets = new Dictionary<string, GameObject>();
    private List<string> knownTargets = new List<string> { "A", "B" }; // 可扩展更多二维码名
    private string currentTargetName = null;
    private string confirmedTargetName = null;

    void OnEnable() => trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    void OnDisable() => trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
            HandleTrackedImage(trackedImage);

        foreach (var trackedImage in args.updated)
            HandleTrackedImage(trackedImage);
    }

    void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;

        if (!knownTargets.Contains(name)) return;

        if (!spawnedTargets.ContainsKey(name))
        {
            GameObject obj = Instantiate(targetPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            obj.name = name;
            spawnedTargets[name] = obj;
            Debug.Log($"🆕 生成目标物：{name}");
        }

        GameObject go = spawnedTargets[name];
        go.SetActive(trackedImage.trackingState == TrackingState.Tracking);
        go.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);

        // 初始状态下不设为当前目标，避免未确认时高亮
        if (string.IsNullOrEmpty(currentTargetName) && trackedImage.trackingState == TrackingState.Tracking)
        {
            currentTargetName = name;
        }

        HighlightTargets();
    }

    public void SwitchTarget(bool toRight)
    {
        if (spawnedTargets.Count == 0) return;

        List<string> available = new List<string>(spawnedTargets.Keys);
        available.Sort();

        int index = available.IndexOf(currentTargetName);
        if (index == -1) index = 0;

        index = toRight ? (index + 1) % available.Count : (index - 1 + available.Count) % available.Count;

        currentTargetName = available[index];
        Debug.Log($"🔁 切换目标为：{currentTargetName}");

        HighlightTargets();
    }

    void HighlightTargets()
    {
        foreach (var kv in spawnedTargets)
        {
            bool isCurrent = kv.Key == currentTargetName;
            bool isConfirmed = kv.Key == confirmedTargetName;
            UpdateTargetVisual(kv.Value, isCurrent, isConfirmed);
        }
    }

    public void ConfirmCurrentTarget()
    {
        if (string.IsNullOrEmpty(currentTargetName)) return;

        confirmedTargetName = currentTargetName;

        if (spawnedTargets.TryGetValue(currentTargetName, out GameObject target))
        {
            UpdateTargetVisual(target, true, true);
            Debug.Log($"✅ 当前目标 {currentTargetName} 已确认！");
        }
    }

    public void CancelCurrentTarget()
    {
        confirmedTargetName = null;
        HighlightTargets();
        Debug.Log($"⛔ 当前目标 {currentTargetName} 已取消确认");
    }

    public string GetCurrentTargetName() => currentTargetName;

    private void UpdateTargetVisual(GameObject obj, bool isCurrent, bool isConfirmed)
    {
        if (obj == null) return;

        // 设置颜色
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            if (isConfirmed)
                rend.material.color = Color.green;
            else if (isCurrent)
                rend.material.color = Color.yellow;
            else
                rend.material.color = Color.white;
        }

        // 设置缩放
        float baseScale = 0.06f;
        float scale = isCurrent ? baseScale * 1.2f : baseScale;
        obj.transform.localScale = new Vector3(scale, scale, scale);

        // 查找 TextMeshPro 并设置状态文字
        TextMeshPro statusText = obj.GetComponentInChildren<TextMeshPro>(true);
        if (statusText != null)
        {
            if (isConfirmed)
                statusText.text = "Move!";
            else if (isCurrent)
                statusText.text = "Target";
            else
                statusText.text = "";

            // 始终朝向主摄像头
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                statusText.transform.LookAt(mainCam.transform);
                statusText.transform.Rotate(0, 180, 0); // 使文字正对用户
            }
        }
    }
}
