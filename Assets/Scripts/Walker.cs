using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum State{
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

    private Vector3 leftFootTargetOffset;
    private Vector3 rightFootTargetOffset;
    private Vector3 leftHandTargetOffset;
    private Vector3 rightHandTargetOffset;
    
    private float leftFootLastForwardMovement = 0f;
    private float rightFootLastForwardMovement = 0f;
    private float elapsedTime=0.0f;

    private  Quaternion startRotation = new Quaternion(0f, 0f, 0f, 0f );
     private  Quaternion rotationChange = new Quaternion(0f, 0f, 0f, 0f );
     private State activeState = State.Walking;
     private  List<int> unobstractedDeg;
     private int? turningDeg;
    Animator animator;
    void Start()
    {
        animator  = GetComponent<Animator>();
        // animator.SetBool("IsIdle", true);
        unobstractedDeg = new List<int>();
        rotationChange = new Quaternion(0f, 0f, 0f, 0f );
        
        leftFootTargetOffset = leftFootTarget.localPosition;
        rightFootTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
        activeState = State.Walking;
    }

    void SetTargetPosition(Transform target, Vector3 offset, float forward, float upwards){
        // Performs the mathematical operations to change the target position using the  arguments given

        Vector3 moveForward = this.transform.InverseTransformVector(target.forward) * forward;
        Vector3 moveUp = this.transform.InverseTransformVector(target.up) * upwards;

        target.localPosition = offset + moveForward + moveUp;
    }



    float moveLeftFootTarget(float adjustedTime){
        float forward = legHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime+0.5f) * 0.2f;
        SetTargetPosition(leftFootTarget,  leftFootTargetOffset, forward, upward);
        float forwardDirection = forward - leftFootLastForwardMovement;
        leftFootLastForwardMovement = forward;
        return forwardDirection;
    }

    float moveRightFootTarget(float adjustedTime){
        float forward = legHorizontalCurve.Evaluate(adjustedTime-1) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime-0.5f) * 0.2f;
        SetTargetPosition(rightFootTarget,  rightFootTargetOffset, forward, upward);
        float forwardDirection = forward - rightFootLastForwardMovement;
        rightFootLastForwardMovement = forward;
        return forwardDirection;
    }

    void moveLeftHandTarget(float adjustedTime){
        float forward = armHorizontalCurve.Evaluate(adjustedTime-1f) * 0.4f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        SetTargetPosition(leftHandTarget,  leftHandTargetOffset, forward, upward);
    }

    void moveRightHandTarget(float adjustedTime){
        float forward = armHorizontalCurve.Evaluate(adjustedTime) * 0.4f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        SetTargetPosition(rightHandTarget,  rightHandTargetOffset, forward, upward);
    }
    
    public void Walk(float adjustedTime){
        // Walking movement
        // move the target that the left foot tip is following
        float  leftLegDirectionforward  = moveLeftFootTarget(adjustedTime ); 
         // move the target that the right foot tip is following
        float rightLegDirectionforward = moveRightFootTarget(adjustedTime);
        // move the target that the left hand is following
        moveLeftHandTarget(adjustedTime); 
        // move the target that the right hand is following
        moveRightHandTarget(adjustedTime); 
        
        // move game object forward when the foot hits the floor 
        RaycastHit hit;
        // find the position where a vertical line starting from the target hits the floor
        bool raycastHittingFloor = Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, 10f );
        // if the leg moves backwards set the target to be on the floor for aesthetical reasons
        // Also when the leg moves backward move the character
        if ( leftLegDirectionforward<0 && raycastHittingFloor ){ 
            leftFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(-leftLegDirectionforward, 0F);
        }
        
        // Same logic as in the code snippet above but for the right leg
        raycastHittingFloor = Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit,  10f);
        if ( rightLegDirectionforward<0 && raycastHittingFloor ){
            rightFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(-rightLegDirectionforward, 0f);
        }
    }

    void Turn(int? deg, Quaternion rotationChange){
        // Coroutines are able to distribute  the execution into many frames
        float turnDuration = Math.Max(Math.Abs((float)Math.Max((float) deg/20.0, 1.0)), 3f);
        Debug.Log("deg: "+ deg);
        Debug.Log("turnDuration" + turnDuration);

        StartCoroutine(RotateOverTime(rotationChange, turnDuration, elapsedTime));
        elapsedTime += Time.deltaTime;
        if (elapsedTime>turnDuration){
            unobstractedDeg = new List<int>();
            turningDeg=0;
            activeState = State.Walking;
            elapsedTime=0f;
            rotationChange = new Quaternion(0f, 0f, 0f, 0f );
        }
    }

    void Stop(){
        animator  = GetComponent<Animator>();
        activeState = State.Stop;
        animator.SetBool("IsIdle", true);
    }

    public List<int> findFeasibleDegrees(Vector3 rayPosition, List<int> unobstractedDeg, bool extendedSearch){
        RaycastHit hitPoint;
        List<int> degrees;
        if (extendedSearch){
            degrees =  new List<int> {-130, -120,- 100, 100, 120, 130};  
        }
        else{
            degrees =  new List<int> {-90, -60, -45, -30, -20, -10, 0, 10, 20, 30, 45, 60, 90};  

        }
        bool hitFlag;

        // Increment the Y position by 1 meter
        foreach (int deg in degrees){
            Quaternion rotation  = Quaternion.Euler(0, deg, 0);
            Vector3 direction = rotation * transform.forward;
            
            hitFlag = Physics.Raycast(rayPosition, direction, out hitPoint,  5f);
            if (!hitFlag && !unobstractedDeg.Contains(deg)){
                unobstractedDeg.Add(deg);
            }
        }
        return unobstractedDeg;
    }
    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !(activeState==State.Turning))
        { 
            turningDeg = -45;
            activeState = State.Turning;
            // ElapsedTime holds the time passed from the start of the turn. When reaching turnDuration we stop turning
            elapsedTime = 0.0f; 
            startRotation = transform.rotation; // Important step: set the current rotation as starting
            rotationChange = Quaternion.AngleAxis((float) turningDeg, this.transform.up); // Set rotation change 45 degrees left
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && !(activeState==State.Turning))
        { 
            turningDeg = 45;
            activeState = State.Turning;
            // ElapsedTime holds the time passed from the start of the turn. When reaching turnDuration we stop turning
            elapsedTime = 0.0f;
            startRotation = transform.rotation; // Important step: set the current rotation as starting
            rotationChange = Quaternion.AngleAxis((float) turningDeg, this.transform.up); // Set rotation change 45 degrees right
        }
        if (Input.GetKeyDown(KeyCode.Space)){
            activeState = State.Stop;
            animator.SetBool("IsIdle", true);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)){
            animator.SetBool("IsIdle", false);
            activeState = State.Walking;
        }

        // check if there is an obstacle in front of the character
        RaycastHit hitPoint;
        bool hitFlag0;
        Vector3 rayPosition = transform.position;
        rayPosition.y += 1.0f;
        Quaternion rotation0  = Quaternion.Euler(0, 0, 0);
        Vector3 direction0 = rotation0 * transform.forward;
        hitFlag0 = Physics.Raycast(rayPosition, direction0, out hitPoint,  4f);
        if (hitFlag0 && activeState != State.Turning){
            unobstractedDeg = findFeasibleDegrees(rayPosition, unobstractedDeg, false);
            if (unobstractedDeg.Count == 0){
                unobstractedDeg = findFeasibleDegrees(rayPosition, unobstractedDeg, true);
            } 

            if (unobstractedDeg.Count > 0)
            {   
                activeState = State.Turning;
                System.Random random = new System.Random();
                // Generate a random index between 0 and the number of elements in the list
                int randomIndex = random.Next(0, unobstractedDeg.Count);

                elapsedTime = 0.0f;
                startRotation = transform.rotation;
                turningDeg = unobstractedDeg[randomIndex];
                rotationChange = Quaternion.AngleAxis((float) turningDeg, this.transform.up); // Set rotation change 45 degrees left
            }
        }
        
    
        // frequency sets the time of a period. Affects the speed of movements.
        float adjustedTime = Time.time * frequency; 
        if (activeState == State.Walking){
            Walk(adjustedTime);
        }
        else if ( activeState == State.Turning){
            Walk(adjustedTime); 
            Turn(turningDeg, rotationChange);
        }
        else{
            Stop();
        }


    }

    private IEnumerator RotateOverTime(Quaternion rotationChange, float duration, float elapsedTime)
    {
        while (elapsedTime < duration)
        {   
            // Slerp is a spherical linear interpolatio  used to smoothly interpolate between the starting rotation and the target rotation.
            transform.rotation = Quaternion.Slerp(startRotation, startRotation * rotationChange, elapsedTime / duration);
            elapsedTime += Time.deltaTime; 
            yield return null; 
        }
    }
}




