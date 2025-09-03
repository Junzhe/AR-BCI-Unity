using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InteractiveSelect : MonoBehaviour
{
    [SerializeField] public TextMeshPro text;
    [SerializeField] GameObject marker; // this.gameobject
    [SerializeField] GameObject turnObj; // turning is only used as an interactive animation.
    public GameObject parentCube;

    private Camera mainCam;
    private Coroutine moveRoutine;
    private Coroutine tiltRoutine;
    private Vector3 pendingTarget;

    // debug
    private Vector3 initialPos;
    private Vector3 nextPos;
    private Vector3 startPos;

    // have 4 different states
    private enum State
    {
        Initial,
        Selected,
        Confirmed,
        Pending,
        Moving
    }
    private State currentState = State.Initial;

    // Start is called before the first frame update
    void Start()
    {
        currentState = State.Selected;

        mainCam = Camera.main;
        StartCoroutine(Bobbing());

        //Debug
        initialPos = transform.position;
        nextPos = transform.position + Vector3.left;
        startPos = Vector3.zero;

    }

    void OnEnable()
    {
        StartCoroutine(Bobbing());
    }

    void OnDisable()
    {
        StopAllCoroutines(); // to be safe, stop old coroutines
    }

    // Update is called once per frame
    void Update()
    {
        faceCam();
        //debug();

        if (currentState != State.Initial)
        {
            if (!text.gameObject.activeSelf)
            {
                text.gameObject.SetActive(true);
            }
        }
    }

    void debug()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            currentState = State.Selected;
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            StartMove(nextPos, initialPos);
            Vector3 temp = nextPos;
            nextPos = initialPos;
            initialPos = temp;
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            CancelMove();
        }
    }

    private void faceCam()
    {
        // the marker should always face where the camera is
        if (mainCam != null)
        {
            marker.transform.LookAt(mainCam.transform);
            Vector3 euler = marker.transform.rotation.eulerAngles;
            euler.x = 0f; // keep upright
            marker.transform.rotation = Quaternion.Euler(euler);
            marker.transform.Rotate(0f, 180f, 0f);
        }
    }

    public void Initialize(Vector3 pos)
    {
        if (currentState == State.Initial)
        {
            currentState = State.Selected;
            Move(pos);
        }
    }

    public void NoTarget()
    {
        // lost the target and we hide the indicator
        text.gameObject.SetActive(false);
        currentState = State.Initial;
    }

    public void StartMove(Vector3 newObjCoord, Vector3 currentCoord)
    {
        if (tiltRoutine != null)
        {
            // if already in coroutine then don't start a new one 
            return;
        }
        tiltRoutine = StartCoroutine(Tilting(newObjCoord, currentCoord));
    }

    public void Move(Vector3 newObjCoord)
    {
        if (moveRoutine != null)
        {
            // if there already is a moving sequence don't do anything
            return;
        }
        moveRoutine = StartCoroutine(Moving(newObjCoord));
    }

    // 不同的Follow重载

    public void Follow(Vector3 target)
    {
        if (currentState != State.Selected)
        {
            currentState = State.Selected;
        }
        if (Vector3.Distance(target, marker.transform.position) < 0.001f)
        {
            // already at the target position, no need to move
            return;
        }
        Move(target);
    }

    public void Follow(Vector3 target, bool forceFollow)
    {
        if (currentState != State.Selected)
        {
            currentState = State.Selected;
        }
        marker.transform.position = target;
    }

    public void Follow(Vector3 target, bool forceFollow, bool isConfirmed)
    {
        if (currentState != State.Confirmed)
        {
            currentState = State.Confirmed;
        }
        marker.transform.position = target;
    }

    public void Follow(bool forceFollow)
    {
        if (currentState != State.Selected)
        {
            currentState = State.Selected;
        }
    }

    public void Follow(bool forceFollow, bool isConfirmed)
    {
        if (currentState != State.Confirmed)
        {
            currentState = State.Confirmed;
        }
    }

    public void CancelMove()
    {
        if (currentState == State.Pending)
        {
            // stop the moving
            currentState = State.Selected;
        }
    }

    IEnumerator Bobbing()
    {
        // When one item is selected, the indicator bobs up and down
        
        float cycle = 2f;

        while (true)
        {
            if (currentState == State.Selected)
            {
                float t = (Mathf.Sin(Time.time * Mathf.PI * 2f / cycle) + 1f) / 2f; // oscillates
                turnObj.transform.localPosition = startPos + Vector3.up * 0.08f * t;
            }
            if (currentState == State.Confirmed)
            {
                float t = (Mathf.Sin(Time.time * Mathf.PI * 2f / (cycle / 2f)) + 1f) / 2f; // oscillates
                turnObj.transform.localPosition = startPos + Vector3.up * 0.02f * t;
            }
            yield return null;
        }
    }

    IEnumerator Moving(Vector3 target)
    {
        currentState = State.Moving; // start moving
        Vector3 start = marker.transform.position;
        float t0 = 0f;
        while (t0 < 0.5f)
        {
            t0 += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, t0 / 0.5f);
            marker.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        marker.transform.position = target; //finish moving and then change the state to selected state
        currentState = State.Selected;
        moveRoutine = null;
    }

    IEnumerator Tilting(Vector3 targetPos, Vector3 currentPos)
    {
        currentState = State.Pending;
        // when recieving signals to move to the next location, the text tilts towards that location for 3 seconds
        // if the signal is still there after 3 seconds, we move the indicator to the new location
        // if the signal dissapears within 3 seconds, we cancel this move.
        pendingTarget = targetPos;

        Quaternion startRot = turnObj.transform.localRotation; // record the initial rotation

        // Because I don't want to hard code "left" or "right", I will change the lookAt pos based on those two coordinate's relative position.
        Vector3 currentVP = Camera.main.WorldToViewportPoint(currentPos);
        Vector3 targetVP = Camera.main.WorldToViewportPoint(targetPos);
        Vector2 dir = (targetVP - currentVP);
        dir.Normalize();

        // calculate the tilting towards that direction
        float maxTilt = 30f;
        Quaternion targetRot = startRot * Quaternion.Euler(-dir.y * maxTilt, dir.x * maxTilt, 0f);

        // tilt
        bool cancelled = false;
        float t = 0f;
        while (t < 3.0f)
        {
            t += Time.deltaTime;
            turnObj.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t / 3.0f);
            if (currentState != State.Pending)
            {
                // if the state changed, that means the user canceled this move, and we stop the tilting and return to normal
                cancelled = true;
                tiltRoutine = null;
                break;
            }
            yield return null;
        }

        // when tilting is finished we change the rotation back.
        turnObj.transform.localRotation = startRot;
        if (!cancelled)
        {
            tiltRoutine = null;
            Move(pendingTarget);
        }
        else
        {
            // back to normal
            currentState = State.Selected;
        }
        yield return null;
    }
}
