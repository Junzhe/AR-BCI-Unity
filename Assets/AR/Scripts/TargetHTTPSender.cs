using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TargetHTTPSender : MonoBehaviour
{
    [Header("目标服务器配置")]
    public string raspberryPiIP = "172.20.10.9";  // 树莓派 IP
    public int port = 5000;

    private string confirmedTargetName = null;
    private Coroutine repeatCoroutine;

    // ✅ 添加的 Start 方法用于日志测试
    void Start()
    {
        Debug.Log("✅ Unity 日志输出测试：TargetHTTPSender Start() 被调用");
    }

    /// <summary>
    /// 外部调用开始发送
    /// </summary>
    public void StartSending(string targetName)
    {
        confirmedTargetName = targetName;

        if (string.IsNullOrEmpty(confirmedTargetName))
        {
            Debug.LogWarning("⚠️ 未提供目标编号，无法发送！");
            return;
        }

        Debug.Log($"📤 开始发送目标编号：{confirmedTargetName}");

        if (repeatCoroutine != null)
            StopCoroutine(repeatCoroutine);

        repeatCoroutine = StartCoroutine(SendTargetPeriodically());
    }

    /// <summary>
    /// 停止发送
    /// </summary>
    public void StopSending()
    {
        Debug.Log("🛑 停止发送目标编号");
        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }

    /// <summary>
    /// 每秒发送一次编号到 HTTP 接口
    /// </summary>
    IEnumerator SendTargetPeriodically()
    {
        while (!string.IsNullOrEmpty(confirmedTargetName))
        {
            string url = $"http://{raspberryPiIP}:{port}/target";
            WWWForm form = new WWWForm();
            form.AddField("target", confirmedTargetName);

            Debug.Log($"🌐 正在向 {url} 发送编号 {confirmedTargetName}");

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ 成功发送目标编号：{confirmedTargetName}");
                }
                else
                {
                    Debug.LogWarning($"❌ HTTP 请求失败：{www.error}");
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}
