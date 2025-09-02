using UnityEngine;
using extOSC;

public class BCISwitchReceiver : MonoBehaviour
{
    public OSCReceiver receiver;
    public string leftAddress = "/left";
    public string rightAddress = "/right";
    public string neutralAddress = "/neutral";
    public ImageTrackingManager imageManager;

    private enum Direction { None, Left, Right }
    private Direction currentDir = Direction.None;

    private float holdStart = 0f;
    public float holdSeconds = 2f; // 持续阈值
    public float amplitude = 0.03f;
    public float speed = 3f;
    public float timeout = 3f;

    private Vector3 basePos;
    private float lastActiveTime = -999f;

    void Start()
    {
        if (!receiver) { Debug.LogError("❌ OSCReceiver 未设置！"); return; }
        receiver.Bind(leftAddress, m => Handle(Direction.Left, m.Values[0].IntValue));
        receiver.Bind(rightAddress, m => Handle(Direction.Right, m.Values[0].IntValue));
        if (!string.IsNullOrEmpty(neutralAddress))
            receiver.Bind(neutralAddress, m => ResetFeedback());
    }

    void Update()
    {
        var current = imageManager?.GetCurrentTargetObject();
        if (current == null) return;

        if (basePos == Vector3.zero) basePos = current.transform.localPosition;

        if (Time.time - lastActiveTime > timeout || currentDir == Direction.None)
        {
            current.transform.localPosition = basePos;
            return;
        }

        float t = Mathf.Sin(Time.time * speed) * amplitude;
        Vector3 offset = Vector3.zero;

        switch (currentDir)
        {
            case Direction.Left: offset = new Vector3(-t, 0, 0); break;
            case Direction.Right: offset = new Vector3(t, 0, 0); break;
        }

        current.transform.localPosition = basePos + offset;
    }

    void Handle(Direction dir, int val)
    {
        var current = imageManager?.GetCurrentTargetObject();
        if (current == null) return;

        if (val == 1)
        {
            currentDir = dir;
            lastActiveTime = Time.time;

            if (holdStart == 0f) holdStart = Time.time;
            else if (Time.time - holdStart >= holdSeconds)
            {
                imageManager?.SwitchTarget(dir == Direction.Right);
                ResetFeedback();
            }
        }
        else ResetFeedback();
    }

    void ResetFeedback()
    {
        currentDir = Direction.None;
        holdStart = 0f;
        var current = imageManager?.GetCurrentTargetObject();
        if (current != null) current.transform.localPosition = basePos;
    }
}
