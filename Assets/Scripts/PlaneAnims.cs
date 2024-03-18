using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAnims : MonoBehaviour
{

    private Animator anim;
    private bool landingGear;
    private float lastGearChange;

	void Start ()
    {
        //Initilises animator
        anim = GetComponent<Animator>();
        lastGearChange = Time.time;

	}

	void Update ()
    {
        //Toggles landing gear on a 2 second cooldown timer
        if ((Time.time - lastGearChange > 2) && Input.GetButton("LandingGear"))
        {
            if (landingGear != true)
            {
                anim.Play("WheelDown");
                landingGear = true;
                lastGearChange = Time.time;
            }
            if (landingGear != false)
            {
                anim.Play("WheelUp");
                landingGear = false;
                lastGearChange = Time.time;
            }
        }
    }

    //Sets propeller speed for animator
    public void setPropellerSpeed(float speed)
    {
        anim.SetFloat("propellerSpeed", speed);
    }
}
