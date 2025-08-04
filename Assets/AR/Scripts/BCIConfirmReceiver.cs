using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class BCIConfirmReceiver : MonoBehaviour
{
    [Header("OSC 配置")]
    public OSCReceiver receiver;
    public string listenAddress = "/confirm";

    [Header("目标管理器")]
    public ImageTrackingManager imageTrackingManager;

    [Header("HTTP 发射器")]
    public TargetHTTPSender targetHttpSender;  // 需要挂载该组件

    [Header("UI 提示组件")]
    public Text statusText;

    private string confirmedTargetName = null;

    void Start()
    {
        if (receiver != null)
        {
            receiver.Bind(listenAddress, OnConfirm);
            Debug.Log($"✅ BCIConfirmReceiver: 已绑定 OSC 地址: {listenAddress}");
        }
        else
        {
            Debug.LogWarning("⚠️ BCIConfirmReceiver: OSCReceiver 未绑定！");
        }
    }

    void OnConfirm(OSCMessage message)
    {
        if (!message.ToFloat(out float value))
        {
            Debug.LogWarning("⚠️ OnConfirm: 无法解析 OSC 浮点值！");
            return;
        }

        Debug.Log($"BCIConfirmReceiver: 接收到 BCI 确认信号值：{value}");

        if (imageTrackingManager == null)
        {
            Debug.LogWarning("⚠️ BCIConfirmReceiver: ImageTrackingManager 未设置！");
            return;
        }

        string name = imageTrackingManager.GetCurrentTargetName();
        Debug.Log($"当前选中的目标编号为：{name}");

        if (value > 0.5f)
        {
            imageTrackingManager.ConfirmCurrentTarget();
            confirmedTargetName = name;

            Debug.Log($"  TargetHTTPSender 开始发送目标编号：{confirmedTargetName}");
            if (targetHttpSender != null)
                targetHttpSender.StartSending(confirmedTargetName);
            else
                Debug.LogWarning("⚠️ BCIConfirmReceiver: TargetHTTPSender 未设置！");

            if (statusText != null)
                statusText.text = $"✅ 目标 {name} 已确认，准备抓取！";
        }
        else
        {
            imageTrackingManager.CancelCurrentTarget();
            confirmedTargetName = null;

            Debug.Log(" 调用 TargetHTTPSender 停止发送");
            if (targetHttpSender != null)
                targetHttpSender.StopSending();

            if (statusText != null)
                statusText.text = $"⛔ 目标 {name} 取消确认";
        }
    }
}
