using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using System;
using Unity.Collections;

[RequireComponent(typeof(ARCameraManager))]
public class ARQRCodeManager : MonoBehaviour
{
    public event Action<string, Vector2> OnQRCodeDetected;

    ARCameraManager cameraManager;
    IBarcodeReader barcodeReader = new BarcodeReader();

    void Awake()
    {
        cameraManager = GetComponent<ARCameraManager>();
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        using (image)
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width, image.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            // 1. 创建 NativeArray<byte>
            var rawTextureData = new NativeArray<byte>(image.GetConvertedDataSize(conversionParams), Allocator.Temp);
            image.Convert(conversionParams, rawTextureData);

            // 2. 将 NativeArray<byte> 拷贝到 byte[]
            byte[] byteArray = rawTextureData.ToArray();

            // 3. 释放 NativeArray
            rawTextureData.Dispose();

            // 用 ZXing 解码
            var result = barcodeReader.Decode(byteArray, image.width, image.height, RGBLuminanceSource.BitmapFormat.RGBA32);
            if (result != null)
            {
                Debug.Log("检测到二维码: " + result.Text);
                OnQRCodeDetected?.Invoke(result.Text, Vector2.zero);
            }
        }
    }
}