using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

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
    private float leftLegDirection=0f;
    private float rightLegDirection=0f;
    private float leftFootLastRightMovement = 0f;
    private float rightFootLastRightMovement = 0f;
    private float turnFlag = 0;
    public float turnDuration = 5.0f; // Duration of the rotation in seconds
    private bool isRotating = true;
    private float elapsedTime=0.0f;
    void Start()
    {
        leftFootTargetOffset = leftFootTarget.localPosition;
        rightFootTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
    }

    void SetTargetPosition(Transform target, Vector3 offset, float forward, float upwards, float right){
        Vector3 moveForward = this.transform.InverseTransformVector(target.forward) * forward;
        Vector3 moveUp = this.transform.InverseTransformVector(target.up) * upwards;
        Vector3 moveRight = this.transform.InverseTransformVector(target.right) * right;

        target.localPosition = offset + moveForward + moveUp + moveRight;
    }

    private void OnCollisionEnter(Collision collided){
        Debug.Log("Collided with " + collided);
        this.turnFlag = 1.0f;
        elapsedTime = 0.0f;
    }

    (float, float) moveLeftFootTarget(float adjustedTime, float right){
        float forward = legHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime+0.5f) * 0.25f;
        SetTargetPosition(leftFootTarget,  leftFootTargetOffset, forward, upward, right);
        float forwardDirection = forward - leftFootLastForwardMovement;
        float rightDirection = right - leftFootLastRightMovement;
        leftFootLastForwardMovement = forward;
        leftFootLastRightMovement = right;
        return (forwardDirection, rightDirection);
    }

    (float, float) moveRightFootTarget(float adjustedTime, float right){
        float forward = legHorizontalCurve.Evaluate(adjustedTime-1) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime-0.5f) * 0.25f;
        SetTargetPosition(rightFootTarget,  rightFootTargetOffset, forward, upward, right);
        float forwardDirection = forward - rightFootLastForwardMovement;
        float rightDirection = right - rightFootLastRightMovement;

        rightFootLastForwardMovement = forward;
        rightFootLastRightMovement = right;
        return (forwardDirection, rightDirection);

    }

    void moveLeftHandTarget(float adjustedTime){
        float forward = armHorizontalCurve.Evaluate(adjustedTime-1f) * 0.4f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        float right = 0.0f;
        SetTargetPosition(leftHandTarget,  leftHandTargetOffset, forward, upward, right);
    }

    void moveRightHandTarget(float adjustedTime){
        float forward = armHorizontalCurve.Evaluate(adjustedTime) * 0.4f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        float right = 0.0f;
        SetTargetPosition(rightHandTarget,  rightHandTargetOffset, forward, upward, right);
    }

    void Update()
    {   
        
        float adjustedTime = Time.time * frequency;
        float right = 0.0f;
        Debug.Log("turnFlag"+ this.turnFlag);

        // Walking movement
        (float  leftLegDirectionforward, float leftLegDirectionright)  = moveLeftFootTarget(adjustedTime, right);
        (float rightLegDirectionforward, float rightLegDirectionright) = moveRightFootTarget(adjustedTime, right);
        moveLeftHandTarget(adjustedTime);
        moveRightHandTarget(adjustedTime);


        // When the character is close to an obstacle he needs to turn
        // Check distance from closest obstacle on degrees -45, +45, -90, 90 , -180, +180
        // The first degree that is feasible is picked
        // if turned 45 degrees check at second 5 10 , 15 if we can turn + 45 again

        // move game object forward when the foot hits the floor
        RaycastHit hit;
        bool raycastHittingFloor = Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, 10f );
        if ( raycastHittingFloor  && leftLegDirectionforward<0){
            leftFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(-leftLegDirectionforward, 0F);
            // this.transform.position += this.transform.right * leftLegDirectionright;
            //  if (turnFlag == 1 && elapsedTime<=turnDuration) {
            //     this.transform.position += this.transform.right * leftLegDirectionforward;
                
            //  }

        }
        
        raycastHittingFloor = Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit,  10f);
        if ( raycastHittingFloor && rightLegDirectionforward<0){
            rightFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(-rightLegDirectionforward, 0f);
            
            //  if (turnFlag == 1 && elapsedTime<=turnDuration) {
            //     this.transform.position += this.transform.right * rightLegDirectionforward;
            //  }

            // this.transform.position += this.transform.right * rightLegDirectionright;

        }
        // if (turnFlag==1){
        //     StartCoroutine(RotateOverTime(Quaternion.Euler(0, 90, 0), turnDuration));
        // }
        // elapsedTime += Time.deltaTime;
        // if (elapsedTime >turnDuration){
        //     turnFlag=0;
        // }

    }

    private IEnumerator RotateOverTime(Quaternion targetRotation, float duration)
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        float elapsedTimeLocal = 0f;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTimeLocal / duration);
            elapsedTimeLocal += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final rotation is set exactly to the target rotation
        transform.rotation = targetRotation;
        isRotating = false;
    }
}




