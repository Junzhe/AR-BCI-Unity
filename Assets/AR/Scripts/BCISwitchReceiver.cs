using UnityEngine;
using extOSC;

public class BCISwitchReceiver : MonoBehaviour
{
    public OSCReceiver receiver;
    public string leftAddress = "/left";
    public string rightAddress = "/right";
    public ImageTrackingManager imageManager;

    void Start()
    {
        if (receiver != null)
        {
            receiver.Bind(leftAddress, OnLeft);
            receiver.Bind(rightAddress, OnRight);
            Debug.Log("✅ 已绑定 /left 与 /right");
        }
        else
        {
            Debug.LogError("❌ OSCReceiver 未设置！");
        }
    }

    void OnLeft(OSCMessage message)
    {
        Debug.Log("🧠 收到 /left 切换指令");
        imageManager?.SwitchTarget(false); // 向左切换目标
    }

    void OnRight(OSCMessage message)
    {
        Debug.Log("🧠 收到 /right 切换指令");
        imageManager?.SwitchTarget(true); // 向右切换目标
    }
}
