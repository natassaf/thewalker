using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public float frequency = 1.5f; 

    private Vector3 leftFootTargetOffset;
    private Vector3 rightFootTargetOffset;
    private Vector3 leftHandTargetOffset;
    private Vector3 rightHandTargetOffset;
    
    private float leftFootLastForwardMovement = 0f;
    private float rightFootLastForwardMovement = 0f;
    public float turnDuration = 5.0f; // Duration of the rotation in seconds
    private float elapsedTime=0.0f;

    private  Quaternion startRotation = new Quaternion(0f, 0f, 0f, 0f );
     private  Quaternion rotationChange = new Quaternion(0f, 0f, 0f, 0f );
     private State activeState = State.Walking;

    void Start()
    {
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
        float upward = legVerticalCurve.Evaluate(adjustedTime+0.5f) * 0.3f;
        SetTargetPosition(leftFootTarget,  leftFootTargetOffset, forward, upward);
        float forwardDirection = forward - leftFootLastForwardMovement;
        leftFootLastForwardMovement = forward;
        return forwardDirection;
    }

    float moveRightFootTarget(float adjustedTime){
        float forward = legHorizontalCurve.Evaluate(adjustedTime-1) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime-0.5f) * 0.3f;
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
        float  leftLegDirectionforward  = moveLeftFootTarget(adjustedTime );
        float rightLegDirectionforward = moveRightFootTarget(adjustedTime);
        moveLeftHandTarget(adjustedTime);
        moveRightHandTarget(adjustedTime);
        
        // move game object forward when the foot hits the floor 
        RaycastHit hit;
        bool raycastHittingFloor = Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, 10f );
        if ( leftLegDirectionforward<0 && raycastHittingFloor ){
            leftFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(-leftLegDirectionforward, 0F);
        }
        
        raycastHittingFloor = Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit,  10f);
        if ( rightLegDirectionforward<0 && raycastHittingFloor ){
            rightFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(-rightLegDirectionforward, 0f);
    

        }
    }

    void Turn(){
        StartCoroutine(RotateOverTime(rotationChange, turnDuration, elapsedTime));
        elapsedTime += Time.deltaTime;
        if (elapsedTime>turnDuration){
            // transform.rotation = startRotation * rotationChange;
            activeState = State.Walking;
            elapsedTime=0f;
        }
    }

    void Stop(){
        
    }

    void Update()
    {   


        if (Input.GetKeyDown(KeyCode.RightArrow) && !(activeState==State.Turning))
        { 
            activeState = State.Turning;
            elapsedTime = 0.0f;
            startRotation = transform.rotation;
            // rotationChange = Quaternion.AngleAxis(90f, this.transform.up);
            rotationChange = Quaternion.AngleAxis(-45f, this.transform.up);

        }
        
        Debug.Log("active state"+ activeState);
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

        
        
        // When the character is close to an obstacle he needs to turn
        // Check distance from closest obstacle on degrees -45, +45, -90, 90 , -180, +180
        // The first degree that is feasible is picked
        // if turned 45 degrees check at second 5 10 , 15 if we can turn + 45 again


    }

    private IEnumerator RotateOverTime(Quaternion rotationChange, float duration, float elapsedTime)
    {
        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, startRotation * rotationChange, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            Debug.Log("elapsedTimeLocal: "+ elapsedTime);

            yield return null; 
        }
    }
}




