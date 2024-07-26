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

    public AnimationCurve horizontalCurve;
    public AnimationCurve verticalCurve;

    private Vector3 leftTargetOffset;
    private Vector3 rightTargetOffset;
    private Vector3 leftHandTargetOffset;
    private Vector3 rightHandTargetOffset;
    public float frequency = 1.2f; 

    private float leftFootLastForwardMovement = 0f;
    private float rightFootLastForwardMovement = 0f;


    void Start()
    {
        leftTargetOffset = leftFootTarget.localPosition;
        rightTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
    }

    void Update()
    {   
        float adjustedTime = Time.time * frequency;
        
        // LEFT FOOT MOVEMENT
        float movementChangeLeftFoot = horizontalCurve.Evaluate(adjustedTime);
        Vector3 movement_z_left_foot = this.transform.InverseTransformVector(leftFootTarget.forward) * movementChangeLeftFoot;
        Vector3 movement_y_left_foot = this.transform.InverseTransformVector(leftFootTarget.up) * verticalCurve.Evaluate(adjustedTime+0.5f);
        
        // RIGHT FOOT MOVEMENT
        float movementChangeRightFoot = horizontalCurve.Evaluate(adjustedTime-1);
        Vector3 movement_z_right_foot = this.transform.InverseTransformVector(rightFootTarget.forward) * movementChangeRightFoot; 
        Vector3 movement_y_right_foot = this.transform.InverseTransformVector(rightFootTarget.up) * verticalCurve.Evaluate(adjustedTime-0.5f);

        // RIGHT HAND MOVEMENT
        Vector3 movement_z_right_hand = this.transform.InverseTransformDirection(rightHandTarget.forward) * horizontalCurve.Evaluate(adjustedTime);
        
        // LEFT HAND MOVEMENT
        Vector3 movement_z_left_hand = this.transform.InverseTransformDirection(rightHandTarget.forward) * horizontalCurve.Evaluate(adjustedTime-1f);

        // UPDATE HANDS LOCAL POSITION
        rightHandTarget.localPosition = rightHandTargetOffset + movement_z_right_hand;
        leftHandTarget.localPosition = leftTargetOffset + movement_z_left_hand;

        //UPDATE FEET LOCAL POSITION
        leftFootTarget.localPosition = leftTargetOffset + movement_z_left_foot + movement_y_left_foot;
        rightFootTarget.localPosition = rightTargetOffset + movement_z_right_foot + movement_y_right_foot;

        float LeftLegDirection = movementChangeLeftFoot - leftFootLastForwardMovement;
        float RightLegDirection = movementChangeRightFoot - rightFootLastForwardMovement;

        // Only when character's feet move backwards we set the foot to stick to the ground
        RaycastHit hit;
        if (LeftLegDirection < 0 && Physics.Raycast(leftFootTarget.position + leftFootTarget.up, -leftFootTarget.up, out hit, Mathf.Infinity )){
            leftFootTarget.position = hit.point;
        }

        if (RightLegDirection<0 && Physics.Raycast(rightFootTarget.position + rightFootTarget.up, -rightFootTarget.up, out hit, Mathf.Infinity)){
            rightFootTarget.position = hit.point;
        }
        leftFootLastForwardMovement = movementChangeLeftFoot;
        rightFootLastForwardMovement = movementChangeRightFoot;

    }

}


