using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

public class TargetHTTPSender : MonoBehaviour
{
    [Header("目标服务器配置")]
    public string raspberryPiIP = "172.20.10.9";  // 树莓派 IP
    public int port = 5000;

    /// <summary>
    /// 一次性发送目标编号，返回服务器响应文本 (OK/FAIL/BUSY/ERROR)
    /// </summary>
    public IEnumerator SendOnce(string targetName, Action<bool, string> onDone = null)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            onDone?.Invoke(false, "INVALID_TARGET");
            yield break;
        }

        string url = $"http://{raspberryPiIP}:{port}/target";
        WWWForm form = new WWWForm();
        form.AddField("target", targetName);
        // 加一个 request_id，避免重复
        form.AddField("request_id", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.timeout = 8; // 超时时间
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onDone?.Invoke(false, $"NETWORK_ERROR: {www.error}");
            }
            else
            {
                // 读取服务器返回的纯文本：OK / FAIL / BUSY / ERROR
                string resp = (www.downloadHandler?.text ?? "").Trim();

                // HTTP 状态码
                int code = (int)www.responseCode;

                if (code == 200 && resp == "OK")
                    onDone?.Invoke(true, "OK");
                else if (resp == "FAIL")
                    onDone?.Invoke(false, "FAIL");
                else if (resp == "BUSY")
                    onDone?.Invoke(false, "BUSY");
                else if (resp == "ERROR")
                    onDone?.Invoke(false, "ERROR");
                else
                    onDone?.Invoke(false, $"UNKNOWN:{resp}");
            }
        }
    }
}
