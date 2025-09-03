using UnityEngine;
using extOSC;

public class BCISwitchReceiver : MonoBehaviour
{
    [Header("OSC 设置")]
    public OSCReceiver receiver;
    public string leftAddress = "/left";
    public string rightAddress = "/right";
    public string neutralAddress = "/neutral";

    [Header("引用")]
    public ImageTrackingManager imageManager;

    [Header("参数")]
    private float holdSeconds = 5.0f;   // 需要持续时间

    private enum Direction { None, Left, Right }
    private Direction currentDir = Direction.None;

    private float holdStart = 0f;
    private bool held = false;
    private float lastActiveTime = -999f;
    private Vector3 basePos;

    void Start()
    {
        if (receiver != null)
        {
            receiver.Bind(leftAddress, OnLeft);
            receiver.Bind(rightAddress, OnRight);
            receiver.Bind(neutralAddress, OnCancel);
        }
        else
        {
            Debug.LogError("❌ OSCReceiver 未设置！");
        }
    }

    void Update()
    {
        Handle(currentDir);
    }

    void OnLeft(OSCMessage message)
    {
        // switch to the block on the left
        currentDir = Direction.Left;
        Handle(currentDir);
    }

    void OnRight(OSCMessage message)
    {
        // switch to the block on the right
        currentDir = Direction.Right;
        Handle(currentDir);
    }

    void OnCancel(OSCMessage message)
    {
        // cancel the switch, whether the switch happened or the switch stopped mid way, return the state to its initial state
        Debug.Log("持续需要中断");
        ResetFeedback();
    }

    void Handle(Direction dir)
    {
        var current = imageManager?.GetCurrentTargetObject();
        if (current == null)
        {
            // lost the block somehow
            //ResetFeedback();
            currentDir = Direction.None;
            held = false;
            return;
        }

        if (currentDir != Direction.None)
        {
            Debug.LogWarning("A Request of confirming this cube is recieved and is being handled");
            // if we have a direction
            imageManager?.PrepareSwitchTarget(dir == Direction.Right);
            if (!held)
            {
                held = true;
                // record current time
                holdStart = Time.time;
            }
            else if (Time.time - holdStart >= holdSeconds)
            {
                Debug.Log($"持续 {holdSeconds}s → 切换 {dir}");
                // make the switch
                imageManager?.SwitchTarget(dir == Direction.Right);
                // already made the one-time switch, "re-initialize" everything.
                held = false;
                ResetFeedback();
            }
        }
        else
        {
            // meaning we should stop the count.
            holdStart = Time.time;
        }
    }

    void ResetFeedback()
    {
        held = false;
        currentDir = Direction.None;
        imageManager?.CancelSwitchTarget();
    }
}
