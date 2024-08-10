using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum State
{
    Walking,
    Turning,
    Stop
}

public class Walker : MonoBehaviour
{
    public Transform leftFootTarget;
    public Transform rightFootTarget;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    public AnimationCurve legHorizontalCurve;
    public AnimationCurve legVerticalCurve;
    public AnimationCurve armHorizontalCurve;
    public AnimationCurve armVerticalCurve;
    
    public float frequency = 1.2f;
    public float turnDuration = 2.0f; // Duration of the rotation in seconds

    private Vector3 leftFootTargetOffset;
    private Vector3 rightFootTargetOffset;
    private Vector3 leftHandTargetOffset;
    private Vector3 rightHandTargetOffset;
    
    private float leftFootLastForwardMovement = 0f;
    private float rightFootLastForwardMovement = 0f;
    private float elapsedTime = 0.0f;

    private Quaternion startRotation = Quaternion.identity;
    private Quaternion rotationChange = Quaternion.identity;
    private State activeState = State.Walking;
    private Animator animator;

    // Reference to the terrain object
    private Terrain terrain;
    private float terrainHeightOffset = 0.0f; // Height offset to keep character above terrain

    void Start()
    {
        animator = GetComponent<Animator>();
        activeState = State.Stop;
        animator.SetBool("IsIdle", true);

        leftFootTargetOffset = leftFootTarget.localPosition;
        rightFootTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;

        // Get the terrain reference
        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active terrain found!");
        }
    }

    void SetTargetPosition(Transform target, Vector3 offset, float forward, float upwards)
    {
        Vector3 moveForward = this.transform.InverseTransformVector(target.forward) * forward;
        Vector3 moveUp = this.transform.InverseTransformVector(target.up) * upwards;

        target.localPosition = offset + moveForward + moveUp;
    }

    float MoveLeftFootTarget(float adjustedTime)
    {
        float forward = legHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime + 0.5f) * 0.2f;
        SetTargetPosition(leftFootTarget, leftFootTargetOffset, forward, upward);
        float forwardDirection = forward - leftFootLastForwardMovement;
        leftFootLastForwardMovement = forward;
        return forwardDirection;
    }

    float MoveRightFootTarget(float adjustedTime)
    {
        float forward = legHorizontalCurve.Evaluate(adjustedTime - 1) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime - 0.5f) * 0.2f;
        SetTargetPosition(rightFootTarget, rightFootTargetOffset, forward, upward);
        float forwardDirection = forward - rightFootLastForwardMovement;
        rightFootLastForwardMovement = forward;
        return forwardDirection;
    }

    void MoveLeftHandTarget(float adjustedTime)
    {
        float forward = armHorizontalCurve.Evaluate(adjustedTime - 1f) * 0.4f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        SetTargetPosition(leftHandTarget, leftHandTargetOffset, forward, upward);
    }

    void MoveRightHandTarget(float adjustedTime)
    {
        float forward = armHorizontalCurve.Evaluate(adjustedTime) * 0.4f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        SetTargetPosition(rightHandTarget, rightHandTargetOffset, forward, upward);
    }

    public void Walk(float adjustedTime)
    {
        float leftLegDirectionForward = MoveLeftFootTarget(adjustedTime);
        float rightLegDirectionForward = MoveRightFootTarget(adjustedTime);
        MoveLeftHandTarget(adjustedTime);
        MoveRightHandTarget(adjustedTime);

        RaycastHit hit;
        bool raycastHittingFloor = Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, 10f);

        if (leftLegDirectionForward < 0 && raycastHittingFloor)
        {
            leftFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Mathf.Max(-leftLegDirectionForward, 0F);
        }

        raycastHittingFloor = Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit, 10f);
        if (rightLegDirectionForward < 0 && raycastHittingFloor)
        {
            rightFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Mathf.Max(-rightLegDirectionForward, 0f);
        }

        // Align with terrain normal
        AlignWithTerrainNormal();
    }

    private void AlignWithTerrainNormal()
    {
        if (terrain == null)
        {
            Debug.LogError("No active terrain found!");
            return;
        }

        Vector3 position = transform.position;
        float terrainHeight = terrain.SampleHeight(position) + terrain.transform.position.y;
        position.y = terrainHeight + terrainHeightOffset;
        transform.position = position;

        // Get terrain normal
        Vector3 terrainNormal = GetTerrainNormal(position);
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, terrainNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private Vector3 GetTerrainNormal(Vector3 position)
    {
        if (terrain.terrainData == null)
        {
            Debug.LogError("No terrain data found!");
            return Vector3.up;
        }

        float x = (position.x - terrain.transform.position.x) / terrain.terrainData.size.x;
        float z = (position.z - terrain.transform.position.z) / terrain.terrainData.size.z;
        return terrain.terrainData.GetInterpolatedNormal(x, z);
    }

    void Turn()
    {
        StartCoroutine(RotateOverTime(rotationChange, turnDuration, elapsedTime));
        elapsedTime += Time.deltaTime;
        if (elapsedTime > turnDuration)
        {
            activeState = State.Walking;
            elapsedTime = 0f;
        }
    }

    void Stop()
    {
        activeState = State.Stop;
        animator.SetBool("IsIdle", true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && activeState != State.Turning)
        {
            activeState = State.Turning;
            elapsedTime = 0.0f;
            startRotation = transform.rotation;
            rotationChange = Quaternion.AngleAxis(-45f, this.transform.up);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && activeState != State.Turning)
        {
            activeState = State.Turning;
            elapsedTime = 0.0f;
            startRotation = transform.rotation;
            rotationChange = Quaternion.AngleAxis(45f, this.transform.up);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            activeState = State.Stop;
            animator.SetBool("IsIdle", true);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            animator.SetBool("IsIdle", false);
            activeState = State.Walking;
        }
        Debug.Log("active state: " + activeState);

        float adjustedTime = Time.time * frequency;
        if (activeState == State.Walking)
        {
            Walk(adjustedTime);
        }
        else if (activeState == State.Turning)
        {
            Walk(adjustedTime);
            Turn();
        }
        else
        {
            Stop();
        }
    }

    private IEnumerator RotateOverTime(Quaternion rotationChange, float duration, float elapsedTime)
    {
        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, startRotation * rotationChange, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
