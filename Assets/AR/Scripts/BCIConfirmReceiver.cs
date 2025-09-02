using UnityEngine;
using extOSC;

public class BCIConfirmReceiver : MonoBehaviour
{
    public OSCReceiver receiver;
    public string confirmAddress = "/lift";
    public ImageTrackingManager imageTrackingManager;

    private float holdStart = 0f;
    public float holdSeconds = 2f;

    public float amplitude = 0.03f;
    public float speed = 3f;
    public float timeout = 3f;

    private Vector3 basePos;
    private float lastActiveTime = -999f;
    private bool active = false;

    void Start()
    {
        if (!receiver) { Debug.LogError("❌ OSCReceiver 未设置！"); return; }
        receiver.Bind(confirmAddress, m => Handle(m.Values[0].IntValue));
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

    void Handle(int val)
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
                imageTrackingManager?.ConfirmCurrentTarget();
                ResetFeedback();
            }
        }
        else ResetFeedback();
    }

    void ResetFeedback()
    {
        active = false;
        holdStart = 0f;
        var current = imageTrackingManager?.GetCurrentTargetObject();
        if (current != null) current.transform.localPosition = basePos;
    }
}
