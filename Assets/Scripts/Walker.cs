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
        // evaluate the transformation of the forward direction of target position in given time
        float forward = legHorizontalCurve.Evaluate(adjustedTime) * 0.3f;

        // evaluate the transformation of the upward direction of target position in given time
        float upward = legVerticalCurve.Evaluate(adjustedTime+0.5f) * 0.2f;

        // set the target position
        SetTargetPosition(leftFootTarget,  leftFootTargetOffset, forward, upward);

        //calculate the change of the position in the forward axis
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
        float turnDuration = Math.Max(Math.Abs((float)Math.Max((float) deg/20.0, 1.0)), 2.5f);

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

    public List<int> FindFeasibleDegrees(Vector3 rayPosition, List<int> unobstractedDeg, bool extendedSearch){
        RaycastHit hitPoint;
        List<int> degrees;
        if (extendedSearch){
            // extended search candidate turns
            degrees =  new List<int> {-130, -120,- 100, 100, 120, 130};  
        }
        else{
            // default candidate turns
            degrees =  new List<int> {-90, -60, -45, -30, -20, -10, 0, 10, 20, 30, 45, 60, 90};  

        }
        bool hitFlag;

        // Increment the Y position by 1 meter
        foreach (int deg in degrees){

            // Ray degrees affect the y axis
            Quaternion rotation  = Quaternion.Euler(0, deg, 0);

            // apply the rotation in relation to the charachers forward direction
            Vector3 direction = rotation * transform.forward;
            
            // check for obstacles in distance of 5 meters
            hitFlag = Physics.Raycast(rayPosition, direction, out hitPoint,  5f);

            // if no obstacle is found add the degree under investigation to the candidate turns list
            if (!hitFlag && !unobstractedDeg.Contains(deg)){
                unobstractedDeg.Add(deg);
            }
        }
        return unobstractedDeg;
    }
    private bool CheckForObstacles(Vector3 rayPosition){
        RaycastHit hitPoint;
        bool hitFlag;

        Quaternion rotation0  = Quaternion.Euler(0, 0, 0);
        Vector3 direction0 = rotation0 * transform.forward;
        hitFlag = Physics.Raycast(rayPosition, direction0, out hitPoint,  4f);
        return hitFlag;
    }

    private int? PickTurningDegree(List<int> unobstractedDeg){
        System.Random random = new System.Random();
        int randomIndex = random.Next(0, unobstractedDeg.Count);
        turningDeg = unobstractedDeg[randomIndex];
        return turningDeg;
    }

    void Update()
    {   
        // Code used only for testing 
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
        // End code used only for testing 

        // check if there is an obstacle in front of the character
        Vector3 rayPosition = transform.position;
        rayPosition.y += 1.0f; // tackles a ray slope issue
        // check if there is  an obstacle in front of the character
        bool hitFlag0 = CheckForObstacles(rayPosition);

        // if there is and no other turn is in progress 
        if (hitFlag0 && activeState != State.Turning){
            
            // find unobstructed turns in the range [-90, 90] that the character can take
            unobstractedDeg = FindFeasibleDegrees(rayPosition, unobstractedDeg, false);

            // if no feasible turn is found extend range and search again
            if (unobstractedDeg.Count == 0){
                bool extendedSearch = true;
                unobstractedDeg = FindFeasibleDegrees(rayPosition, unobstractedDeg, extendedSearch);
            } 

            if (unobstractedDeg.Count > 0)
            {   
                // Pick one of the turns from the list
                turningDeg = PickTurningDegree(unobstractedDeg);

                // set the variables needed to turn
                activeState = State.Turning;
                elapsedTime = 0.0f;
                startRotation = transform.rotation;
                rotationChange = Quaternion.AngleAxis((float) turningDeg, this.transform.up); 
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




