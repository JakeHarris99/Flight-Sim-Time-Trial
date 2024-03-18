using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    //Physics Modifiers
    [SerializeField] private float maximumEngineOutput = 40f; //Max Engine Output
    [SerializeField] private float wingLift = 0.002f; //Lift Produced By Wings
    [SerializeField] private float liftSpeedCutoff = 300; //Speed At Which Lift Stops
    [SerializeField] private float rollInputModifier = 1f; //Roll Speed
    [SerializeField] private float pitchInputModifer = 1f; //Pitch Speed
    [SerializeField] private float yawInputModifier = 0.2f; //Yaw Speed
    [SerializeField] private float bankedTurnModifier = 0.5f; //Amount of Turn Produced By Banking
    [SerializeField] private float aerodynamicModifer = 0.02f; //Effect of Aerodynamics On Speed
    [SerializeField] private float automaticPitchingModifier = 0.5f; //Amount of Pitch Produced By Banking
    [SerializeField] private float automaticRollLevellingModifer = 0.2f; //Speed of Roll Levelling
    [SerializeField] private float automaticPitchLevellingModifier = 0.2f; //Speed of Pitch Levelling
    [SerializeField] private float airBrakesDragModifier = 3f; //Drag Multiplier Whilst Brakes Are Active
    [SerializeField] private float throttleAccelerationSpeed = 0.3f; //Speed of Throttle Change
    [SerializeField] private float dragSpeedIncreaseModifier = 0.001f; //Drag Increase With Speed
    [SerializeField] private Camera FPPCam;
    [SerializeField] private Camera TPPCam;

    //Current Values
    private float altitude; //Current Altitude
    private float throttle; //Current Throttle Amount
    private bool airBrakesActive; //Air Brakes On/Off
    private float speed; //Current Forward Speed (Not Velocity)
    private float engineOutput; //Current Engine Power
    private float rollAngle; //Current Roll Angle
    private float pitchAngle; //Current Pitch Angle
    private float rollInput; //Current Roll Input
    private float pitchInput; //Current Pitch Input
    private float yawInput; //Current Yaw Input
    private float throttleInput; //Current Throttle Input

    private Rigidbody rb; //Plane's Rigidbody
    private float startingDrag; //Drag Value On Start
    private float startingAngularDrag; //Angular Drag On Start
    private float directionVelocityRatio; //A Dot Product/Comparison of the direction facing and the velocity of the plane
    private float bankedTurnAmount; //A value between -1 and 1 used a modifier for banked turns
    private float startingMaximumEngineOutput; //Maximum Engine Output On Start
    private float startingRollInputModifier; //Roll Input Modifier On Start
    private float startingPitchInputModifier; //Pitch Input Modifier On Start
    private float startingYawInputModifier; //Yaw Input Modifier On Start

    private float lastCamChange; //Time Camera Was Last Changed
    private Vector3 startingPosition; //Starting Position Of Plane
    private float lastSpeedBuff; //Last Time Speed Buff Activated
    private bool speedBuffOn; //Speed Buff On/Off
    private float lastInvertedBuff; //Last Time Inverted Buff Activated
    private bool invertedBuffOn;//Inverted Buff On/Off
    private float lastMobilityBuff; //Last Time Mobility Buff Activated
    private bool mobilityBuffOn;//Mobility Buff On/Off


    void Start ()
    {
        //Stores Original Values and sets booleans to false
        rb = GetComponent<Rigidbody>();
        startingDrag = rb.drag;
        startingAngularDrag = rb.angularDrag;
        lastCamChange = Time.time;
        startingMaximumEngineOutput = maximumEngineOutput;
        startingPosition = transform.position;
        lastSpeedBuff = Time.time;
        speedBuffOn = false;
        lastInvertedBuff = Time.time;
        invertedBuffOn = false;
        lastMobilityBuff = Time.time;
        mobilityBuffOn = false;

	}
	
	void Update ()
    {
        //Gets Inputs and Assigns Them based on the inverted buff boolean
        if(invertedBuffOn)
        {
            rollInput = -Input.GetAxis("Roll");
            pitchInput = -Input.GetAxis("Pitch");
            airBrakesActive = Input.GetButton("AirBrakes");
            yawInput = -Input.GetAxis("Yaw");
            throttleInput = Input.GetAxis("Throttle");
        }
        else
        {
            rollInput = Input.GetAxis("Roll");
            pitchInput = Input.GetAxis("Pitch");
            airBrakesActive = Input.GetButton("AirBrakes");
            yawInput = Input.GetAxis("Yaw");
            throttleInput = Input.GetAxis("Throttle");
        } 

        //Runs Calculation Methods
        PitchAndRollAngles();
        AeroLevelling();
        GetSpeed();
        EngineControl();
        CreateDrag();
        AerodynamicStability();
        LiftAndThrust();
        PitchYawAndRoll();
        Altitude();

        //Swaps camera positions with a 1 second cooldown timer
        if((Time.time - lastCamChange > 1) && Input.GetButton("CameraChange"))
        {
            FPPCam.enabled = !FPPCam.enabled;
            TPPCam.enabled = !TPPCam.enabled;
            lastCamChange = Time.time;
        }
        //Creates ratio between current power and max and parses it to methods in the manager
        GetComponent<PlaneAnims>().setPropellerSpeed(engineOutput / startingMaximumEngineOutput);
        GetComponent<PlaneAudio>().setPropAudio(Mathf.Clamp01(engineOutput / startingMaximumEngineOutput));
        //Toggles buffs off after their duration timer
        if((Time.time - lastSpeedBuff > 5) && speedBuffOn)
        {
            speedBuff();
        }
        if ((Time.time - lastInvertedBuff > 5) && invertedBuffOn)
        {
            invertedBuff();
        }
        if ((Time.time - lastMobilityBuff > 5) && mobilityBuffOn)
        {
            mobilityBuff();
        }
    }

    private void PitchAndRollAngles()
    {
        //Finds the forward facing vector and removes the y element
        Vector3 forwardVector = transform.forward;
        forwardVector.y = 0;
        forwardVector.Normalize();

        //Converts vector into local vector and calculates the angle between the x axis and the vector (localForwardVector.y, localForwardVector.z)
        Vector3 localForwardVector = transform.InverseTransformDirection(forwardVector);
        pitchAngle = Mathf.Atan2(localForwardVector.y, localForwardVector.z);

        //Creates a right facing vector, converts it to a local vector and then calculates the angle between the x axis and the vector (localRightVector.y, localRightVector.x)
        Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector);
        Vector3 localRightVector = transform.InverseTransformDirection(rightVector);
        rollAngle = Mathf.Atan2(localRightVector.y, localRightVector.x);

    }

    private void AeroLevelling()
    {
        //Creates a value between -1 and 1 by taking the Sin of rollAngle
        bankedTurnAmount = Mathf.Sin(rollAngle);

        //When there is no roll input, reduce the roll angle by a modifier
        if(rollInput == 0f)
        {
            rollInput = -rollAngle * automaticRollLevellingModifer;
        }

        //When there is no pitch input, reduce the pitch angle by a modifier as well as automatically pitch based of the squared value of banked amount
        if(pitchInput == 0f)
        {
            pitchInput = -pitchAngle * automaticPitchLevellingModifier;
            pitchInput -= Mathf.Abs(bankedTurnAmount * bankedTurnAmount * automaticPitchingModifier);
        }
    }

    private void GetSpeed()
    {
        //Simply convert the velocity vector to local and use if to make sure it doesn't become negative
        if(transform.InverseTransformDirection(rb.velocity).z < 0)
        {
            speed = 0;
        }
        else
        {
            speed = transform.InverseTransformDirection(rb.velocity).z;
        }
    }

    private void EngineControl()
    {
        //Allow throttle to increase and decrese by modifer over time (clamped to a min and max of 0 and 1)
        throttle = Mathf.Clamp(throttle + throttleInput * Time.deltaTime * throttleAccelerationSpeed, 0, 1);
        engineOutput = throttle * maximumEngineOutput;
    }

    private void CreateDrag()
    {
        //Create an amount of additional drag based on the speed
        float bonusDragSpeed = rb.velocity.magnitude * dragSpeedIncreaseModifier;
        //Check if airbrakes are on and apply the multiplier if so
        if(airBrakesActive)
        {
            rb.drag = airBrakesDragModifier * (startingDrag + bonusDragSpeed);
        }
        else
        {
            rb.drag = startingDrag + bonusDragSpeed;
        }
        //angular drag is multiplied by the forward speed
        rb.angularDrag = startingAngularDrag * speed;
    }

    private void AerodynamicStability()
    {
        //Compare direction facing with velocity
        directionVelocityRatio = Vector3.Dot(transform.forward, rb.velocity.normalized);
        directionVelocityRatio *= directionVelocityRatio;
        //Adjuct velocity by lerping between the current velocity and proposed velocity based on the speed, angle and strength of aerodynamics
        Vector3 adjustedVelocity = Vector3.Lerp(rb.velocity, transform.forward * speed, directionVelocityRatio * speed * aerodynamicModifer * Time.deltaTime);
        rb.velocity = adjustedVelocity;
        //Lerp the rotation spherically between current rotation and the velocity direction by the aerodynamic strength
        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(rb.velocity, transform.up), aerodynamicModifer * Time.deltaTime);

    }

    private void LiftAndThrust()
    {
        //Create a blank vector3 to add forces to
        Vector3 forces = Vector3.zero;
        //Add engine force in forward direction
        forces += engineOutput * transform.forward;
        //Calculate the lift direction vector
        Vector3 liftDirection = Vector3.Cross(rb.velocity, transform.right).normalized;
        //Inverse lerps between the max speed for lift and 0 by the current speed to create a lift ratio
        float decreasingLift = 0;
        if (speed > 0)
        {
            decreasingLift = (liftSpeedCutoff / Mathf.Clamp(speed, 0, liftSpeedCutoff))/liftSpeedCutoff;
        }
        //lift strength is calculated based on a product of the square of speed (to create a curve with the decreasing lift modifier), the wing lift strength, the lift ratio and direction to velocity ratio
        float liftStrength = speed * speed * wingLift * decreasingLift * directionVelocityRatio;
        //the product of lift strength and direction is added to the forces accumulator and is assigned to the rigidbody
        forces += liftStrength * liftDirection;
        rb.AddForce(forces);
    }

    private void PitchYawAndRoll()
    {
        //Accumulates torque forces based on inputs and direction vectors
        Vector3 torque = Vector3.zero;
        torque += pitchInput * pitchInputModifer * transform.right;
        torque += yawInput * yawInputModifier * transform.up;
        torque += -rollInput * rollInputModifier * transform.forward;
        torque += bankedTurnAmount * bankedTurnModifier * transform.up;

        //apply torque values
        rb.AddTorque(torque * speed * directionVelocityRatio);
    }

    private void Altitude()
    {
        //Use a raycast to check the altitude of the plane
        var ray = new Ray(transform.position, -Vector3.up);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            altitude = hit.distance;
        }
        else
        {
            altitude = 0;
        }
    }

    //resets planes velocity, rotation and position
    public void resetPlane()
    {
        rb.velocity = Vector3.zero;
        engineOutput = 0;
        throttle = 0;
        transform.rotation = Quaternion.identity;
        transform.position = startingPosition;
    }

    //toggles speed buff (multiplies max engine output by 5)
    public void speedBuff()
    {
        if(speedBuffOn)
        {
            maximumEngineOutput /= 5;
            speedBuffOn = false;
        }
        else
        {
            maximumEngineOutput *= 5;
            speedBuffOn = true;
            lastSpeedBuff = Time.time;
        }
        
    }

    //toggles inverted buff (inverts controls)
    public void invertedBuff()
    {
        if(invertedBuffOn)
        {
            invertedBuffOn = false;
        }
        else
        {
            invertedBuffOn = true;
            lastInvertedBuff = Time.time;
        }
    }

    //toggles mobility buff (triples turning speeds)
    public void mobilityBuff()
    {
        if(mobilityBuffOn)
        {
            mobilityBuffOn = false;
            startingPitchInputModifier /= 3;
            startingYawInputModifier /= 3;
            startingRollInputModifier /= 3;
        }
        else
        {
            mobilityBuffOn = true;
            startingPitchInputModifier *= 3;
            startingYawInputModifier *= 3;
            startingRollInputModifier *= 3;
            lastMobilityBuff = Time.time;
        }
    }
}
