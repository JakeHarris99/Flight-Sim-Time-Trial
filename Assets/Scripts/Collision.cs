using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {

    //Detects collision between plane and any other object
    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        //excludes terrain to avoid death on takeoff
        if (collision.gameObject.tag != "Terrain")
        {
            //calls takeDamage method from the manager script
            GameObject.FindGameObjectWithTag("Scripts").GetComponent<Manager>().takeDamage();
        }
    }
}
