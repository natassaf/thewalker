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


    void Start()
    {
        leftTargetOffset = leftFootTarget.localPosition;
        rightTargetOffset = rightFootTarget.localPosition;

        leftHandTargetOffset = leftHandTarget.localPosition;
        rightHandTargetOffset = rightHandTarget.localPosition;
    }

    // Update is called once per frame
    void Update()
    {   
        Vector3 movement_z_left_foot = this.transform.InverseTransformVector(leftFootTarget.forward) * horizontalCurve.Evaluate(Time.time);
        Vector3 movement_y_left_foot = this.transform.InverseTransformVector(leftFootTarget.up) * verticalCurve.Evaluate(Time.time+0.5f);

        Vector3 movement_z_right_foot = this.transform.InverseTransformVector(rightFootTarget.forward) * horizontalCurve.Evaluate(Time.time-1); 
        Vector3 movement_y_right_foot = this.transform.InverseTransformVector(rightFootTarget.up) * verticalCurve.Evaluate(Time.time-0.5f);

        Vector3 movement_z_right_hand = this.transform.InverseTransformDirection(rightHandTarget.forward) * horizontalCurve.Evaluate(Time.time);
        Vector3 movement_z_left_hand = this.transform.InverseTransformDirection(rightHandTarget.forward) * horizontalCurve.Evaluate(Time.time-1f);

        rightHandTarget.localPosition = rightHandTargetOffset + movement_z_right_hand;
        leftHandTarget.localPosition = leftTargetOffset + movement_z_left_hand;


        leftFootTarget.localPosition = leftTargetOffset + movement_z_left_foot + movement_y_left_foot;
        rightFootTarget.localPosition = rightTargetOffset + movement_z_right_foot + movement_y_right_foot;

        
    }

}


