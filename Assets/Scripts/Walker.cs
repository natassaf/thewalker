using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.AI;

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
    public float turnDuration = 2.0f; // Duration of the rotation in seconds
    private float elapsedTime=0.0f;

    private  Quaternion startRotation = new Quaternion(0f, 0f, 0f, 0f );
     private  Quaternion rotationChange = new Quaternion(0f, 0f, 0f, 0f );
     private State activeState = State.Walking;
    Animator animator;
    void Start()
    {
        
        animator  = GetComponent<Animator>();
        activeState = State.Stop;
        animator.SetBool("IsIdle", true);
        
        leftFootTargetOffset = leftFootTarget.localPosition;
        rightFootTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
    }

    void SetTargetPosition(Transform target, Vector3 offset, float forward, float upwards){
        // Performs the mathematical operations to change the target position using the  arguments given

        Vector3 moveForward = this.transform.InverseTransformVector(target.forward) * forward;
        Vector3 moveUp = this.transform.InverseTransformVector(target.up) * upwards;

        target.localPosition = offset + moveForward + moveUp;
    }

    // private void OnCollisionEnter(Collision collided){
    //     Debug.Log("Collided with " + collided);
    //     this.turnFlag = 1.0f;
    //     elapsedTime = 0.0f;
    // }

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

    void Turn(){
        // Coroutines are able to distribute  the execution into many frames
        StartCoroutine(RotateOverTime(rotationChange, turnDuration, elapsedTime));
        elapsedTime += Time.deltaTime;
        if (elapsedTime>turnDuration){
            activeState = State.Walking;
            elapsedTime=0f;
        }
    }

    void Stop(){
        animator  = GetComponent<Animator>();
        activeState = State.Stop;
        animator.SetBool("IsIdle", true);
    }

    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !(activeState==State.Turning))
        { 
            activeState = State.Turning;
            // ElapsedTime holds the time passed from the start of the turn. When reaching turnDuration we stop turning
            elapsedTime = 0.0f; 
            startRotation = transform.rotation; // Important step: set the current rotation as starting
            rotationChange = Quaternion.AngleAxis(-45f, this.transform.up); // Set rotation change 45 degrees left
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && !(activeState==State.Turning))
        { 
            activeState = State.Turning;
            // ElapsedTime holds the time passed from the start of the turn. When reaching turnDuration we stop turning
            elapsedTime = 0.0f;
            startRotation = transform.rotation; // Important step: set the current rotation as starting
            rotationChange = Quaternion.AngleAxis(45f, this.transform.up); // Set rotation change 45 degrees right
        }
        if (Input.GetKeyDown(KeyCode.Space)){
            activeState = State.Stop;
            animator.SetBool("IsIdle", true);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)){
            animator.SetBool("IsIdle", false);
            activeState = State.Walking;
        }
        Debug.Log("active state"+ activeState);
        List<int> degrees =  new List<int> {-90, -60, -45, 0, 45, 60, 90};
        RaycastHit hitPoint;
        bool hitFlag0;
        Debug.Log("this.transform.position:" + this.transform.position);
        Vector3 currentPosition = transform.position;

        // Increment the Y position by 1 meter
        currentPosition.y += 1.0f;

        foreach (int deg in degrees){
            Quaternion rotation  = Quaternion.Euler(0, deg, 0);
            Vector3 direction = rotation * Vector3.forward;
            
            hitFlag0 = Physics.Raycast(currentPosition, direction, out hitPoint,  Mathf.Infinity);
            if (hitPoint.collider.tag == "Obstacle"){
                Debug.Log("deg: "+ deg + "tag: " + hitPoint.collider.tag);

            }
            Debug.DrawRay(currentPosition, direction * 100, Color.blue, 1.0f);

        }
       

        // frequency sets the time of a period. Affects the speed of movements.
        float adjustedTime = Time.time * frequency; 
        if (activeState == State.Walking){
            Walk(adjustedTime);
        }
        else if ( activeState == State.Turning){
            Walk(adjustedTime); 
            Turn();
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




