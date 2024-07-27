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
    public float frequency = 1f; 

    private Vector3 leftFootTargetOffset;
    private Vector3 rightFootTargetOffset;
    private Vector3 leftHandTargetOffset;
    private Vector3 rightHandTargetOffset;
    
    private float leftFootLastForwardMovement = 0f;
    private float rightFootLastForwardMovement = 0f;


    void Start()
    {
        leftFootTargetOffset = leftFootTarget.localPosition;
        rightFootTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
    }

    void SetTargetPosition(Transform target, Vector3 offset, float movementChangeHor, float movementChangeVer){
        Vector3 positionZ = this.transform.InverseTransformVector(leftFootTarget.forward) * movementChangeHor;
        Vector3 positionY = this.transform.InverseTransformVector(leftFootTarget.up) * movementChangeVer;
        target.localPosition = offset + positionZ + positionY;
    }

    void Update()
    {   
        float adjustedTime = Time.time * frequency;

        // LEFT FOOT MOVEMENT
        float movementChangeLeftFootHor = legHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float movementChangeLeftFootVer = legVerticalCurve.Evaluate(adjustedTime+0.5f) * 0.2f;
        SetTargetPosition(leftFootTarget,  leftFootTargetOffset, movementChangeLeftFootHor, movementChangeLeftFootVer);
        
        // RIGHT FOOT MOVEMENT
        float movementChangeRightFootHor = legHorizontalCurve.Evaluate(adjustedTime-1) * 0.3f;
        float movementChangeRightFootVer = legVerticalCurve.Evaluate(adjustedTime-0.5f) * 0.2f;
        SetTargetPosition(rightFootTarget,  rightFootTargetOffset, movementChangeRightFootHor, movementChangeRightFootVer);

        // RIGHT HAND MOVEMENT
        float movementChangeRightHandHor = armHorizontalCurve.Evaluate(adjustedTime) * 0.3f;
        float movementChangeRightHandVer = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        SetTargetPosition(rightHandTarget,  rightHandTargetOffset, movementChangeRightHandHor, movementChangeRightHandVer);

        // LEFT HAND MOVEMENT
        float movementChangeLeftHandHor = armHorizontalCurve.Evaluate(adjustedTime-1f) * 0.3f;
        float movementChangeLeftHandVer = armVerticalCurve.Evaluate(adjustedTime) * 0.01f;
        SetTargetPosition(leftHandTarget,  leftHandTargetOffset, movementChangeLeftHandHor, movementChangeLeftHandVer);

        float LeftLegDirection = movementChangeLeftFootHor - leftFootLastForwardMovement;
        float RightLegDirection = movementChangeRightFootVer - rightFootLastForwardMovement;

        // Only when character's feet move backwards we set the foot to stick to the ground
        RaycastHit hit;
        if (LeftLegDirection < 0 && Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, Mathf.Infinity )){
            leftFootTarget.position = hit.point;
            Debug.Log("this.transform.forward: " + this.transform.forward);
            Debug.Log("LeftLegDirection: " + LeftLegDirection );
            this.transform.position += this.transform.forward * Math.Abs(LeftLegDirection);
        }

        if (RightLegDirection<0 && Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit, Mathf.Infinity)){
            rightFootTarget.position = hit.point;
            this.transform.position += this.transform.forward * Math.Abs(RightLegDirection);

        }
        leftFootLastForwardMovement = movementChangeLeftFootHor;
        rightFootLastForwardMovement = movementChangeRightFootVer;

    }

}


