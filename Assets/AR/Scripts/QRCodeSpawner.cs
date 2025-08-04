using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class QRCodeSpawner : MonoBehaviour
{
    public ARSessionOrigin sessionOrigin;
    public ARQRCodeManager qrCodeManager;
    public GameObject spawnPrefab;

    bool spawned = false;

    void OnEnable()
    {
        if (qrCodeManager != null)
            qrCodeManager.OnQRCodeDetected += HandleQRCode;
    }

    void OnDisable()
    {
        if (qrCodeManager != null)
            qrCodeManager.OnQRCodeDetected -= HandleQRCode;
    }

    void HandleQRCode(string code, Vector2 pos)
    {
        if (spawned) return; // 只生成一次
        spawned = true;
        // 在摄像机前方 0.5 米生成
        Vector3 spawnPos = sessionOrigin.camera.transform.position + sessionOrigin.camera.transform.forward * 0.5f;
        Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
        Debug.Log("根据二维码生成了物体: " + code);
    }
}