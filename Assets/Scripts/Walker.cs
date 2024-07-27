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
        Quaternion targetRotation = transform.localRotation * Quaternion.Euler(0, -90, 0);
        transform.localRotation = targetRotation;
        // leftFootTarget.localRotation = targetRotation;
        // rightFootTarget.localRotation = targetRotation;

        // leftFootTargetOffset = leftFootTarget.localPosition;
        // rightFootTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
    }

    float moveLeftFootTarget(float adjustedTime){
        float forward = legHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime+0.5f) * 0.2f;
        float right = 0.0f;
        SetTargetPosition(leftFootTarget,  leftFootTargetOffset, forward, upward, right);
        float forwardDirection = forward - leftFootLastForwardMovement;
        leftFootLastForwardMovement = forward;
        return forwardDirection;
    }

    float moveRightFootTarget(float adjustedTime){
        float forward = legHorizontalCurve.Evaluate(adjustedTime-1) * 0.3f;
        float upward = legVerticalCurve.Evaluate(adjustedTime-0.5f) * 0.2f;
        float right = 0.0f;
        SetTargetPosition(rightFootTarget,  rightFootTargetOffset, forward, upward, right);
        float forwardDirection = forward - rightFootLastForwardMovement;
        rightFootLastForwardMovement = forward;
        return forwardDirection;

    }

    void moveLeftHandTarget(float adjustedTime){
        float forward = armHorizontalCurve.Evaluate(adjustedTime-1f) * 0.3f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        float right = 0.0f;
        SetTargetPosition(leftHandTarget,  leftHandTargetOffset, forward, upward, right);
    }

    void moveRightHandTarget(float adjustedTime){
        float forward = armHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float upward = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        float right = 0.0f;
        SetTargetPosition(rightHandTarget,  rightHandTargetOffset, forward, upward, right);
    }

    void Update()
    {   
        
        float adjustedTime = Time.time * frequency;

        leftLegDirection = moveLeftFootTarget(adjustedTime);
        rightLegDirection = moveRightFootTarget(adjustedTime);
        moveLeftHandTarget(adjustedTime);
        moveRightHandTarget(adjustedTime);


        // Only when character's feet move backwards we set the foot to stick to the ground
        RaycastHit hit;
        if ( Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, Mathf.Infinity )){
            leftFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(leftLegDirection, 0f);
        }

        if (  Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit, Mathf.Infinity)){
            rightFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Max(rightLegDirection, 0f);
        }


    }

}


