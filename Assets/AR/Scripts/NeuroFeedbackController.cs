using UnityEngine;

public class NeuroFeedbackController : MonoBehaviour
{
    public enum Direction { None, Left, Right, Lift }
    public Direction currentDir = Direction.None;

    private Vector3 basePos;
    private float lastActiveTime = -999f;

    [Header("反馈参数")]
    public float amplitude = 0.03f;  // 位移幅度 (m)
    public float speed = 3f;         // 摆动频率
    public float timeout = 3f;       // 信号中断超时

    void Start()
    {
        //basePos = transform.localPosition;
    }

    void Update()
    {
        if (Time.time - lastActiveTime > timeout || currentDir == Direction.None)
        {
            //transform.localPosition = basePos; //TODO
            return;
        }

        float t = Mathf.Sin(Time.time * speed) * amplitude;
        Vector3 offset = Vector3.zero;

        switch (currentDir)
        {
            case Direction.Left: offset = new Vector3(-t, 0, 0); break;
            case Direction.Right: offset = new Vector3(t, 0, 0); break;
            case Direction.Lift: offset = new Vector3(0, t, 0); break;
        }

        //transform.localPosition = basePos + offset;
    }

    public void Activate(Direction dir)
    {
        currentDir = dir;
        lastActiveTime = Time.time;
    }

    public void Deactivate()
    {
        currentDir = Direction.None;
    }
}
