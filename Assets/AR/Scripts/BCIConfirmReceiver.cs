using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class BCIConfirmReceiver : MonoBehaviour
{
    [Header("OSC 配置")]
    public OSCReceiver receiver;
    public string confirmAddress = "/lift";   // 新版
    public string legacyAddress = "/confirm"; // 兼容旧版

    [Header("目标管理器")]
    public ImageTrackingManager imageTrackingManager;

    [Header("HTTP 发射器")]
    public TargetHTTPSender targetHttpSender;

    [Header("UI 提示组件")]
    public Text statusText;

    [Header("参数")]
    public float holdSeconds = 2f;
    public float amplitude = 0.03f;
    public float speed = 3f;
    public float timeout = 3f;

    private float holdStart = 0f;
    private float lastActiveTime = -999f;
    private bool active = false;
    private Vector3 basePos;

    private string confirmedTargetName = null;

    void Start()
    {
        if (!receiver)
        {
            Debug.LogError("❌ BCIConfirmReceiver: OSCReceiver 未设置！");
            return;
        }

        receiver.Bind(confirmAddress, m => HandleLift(m.Values[0].IntValue));
        receiver.Bind(legacyAddress, HandleLegacy);

        Debug.Log($"✅ BCIConfirmReceiver 已绑定 {confirmAddress} 和 {legacyAddress}");
    }

    void Update()
    {
        var current = imageTrackingManager?.GetCurrentTargetObject();
        if (current == null) return;

        if (basePos == Vector3.zero) basePos = current.transform.localPosition;

        if (!active || Time.time - lastActiveTime > timeout)
        {
            current.transform.localPosition = basePos;
            return;
        }

        float t = Mathf.Sin(Time.time * speed) * amplitude;
        current.transform.localPosition = basePos + new Vector3(0, t, 0);
    }

    void HandleLift(int val)
    {
        var current = imageTrackingManager?.GetCurrentTargetObject();
        if (current == null) return;

        if (val == 1)
        {
            active = true;
            lastActiveTime = Time.time;

            if (holdStart == 0f) holdStart = Time.time;
            else if (Time.time - holdStart >= holdSeconds)
            {
                ConfirmTarget();
                ResetFeedback();
            }
        }
        else
        {
            CancelTarget();
            ResetFeedback();
        }
    }

    void HandleLegacy(OSCMessage message)
    {
        if (!message.ToFloat(out float value))
        {
            Debug.LogWarning("⚠️ HandleLegacy: 无法解析 OSC 浮点值！");
            return;
        }

        if (value > 0.5f) ConfirmTarget();
        else CancelTarget();
    }

    void ConfirmTarget()
    {
        string name = imageTrackingManager?.GetCurrentTargetName();
        if (string.IsNullOrEmpty(name)) return;

        imageTrackingManager?.ConfirmCurrentTarget();
        confirmedTargetName = name;

        Debug.Log($"✅ 目标 {name} 已确认，HTTP 一次发送");
        if (targetHttpSender != null)
        {
            StartCoroutine(targetHttpSender.SendOnce(confirmedTargetName,
                (ok, resp) => Debug.Log($"📡 HTTP 返回: {resp}")));
        }

        if (statusText != null)
            statusText.text = $"✅ 目标 {name} 已确认，准备抓取！";
    }

    void CancelTarget()
    {
        string name = imageTrackingManager?.GetCurrentTargetName();
        if (string.IsNullOrEmpty(name)) return;

        imageTrackingManager?.CancelCurrentTarget();
        confirmedTargetName = null;

        Debug.Log("⛔ 目标取消确认，不再发送 HTTP");
        if (statusText != null)
            statusText.text = $"⛔ 目标 {name} 取消确认";
    }

    void ResetFeedback()
    {
        active = false;
        holdStart = 0f;
        var current = imageTrackingManager?.GetCurrentTargetObject();
        if (current != null) current.transform.localPosition = basePos;
    }
}
