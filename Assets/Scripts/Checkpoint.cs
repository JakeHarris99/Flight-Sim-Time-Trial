using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    //Detects collisions with the player
    private void OnTriggerEnter(Collider other)
    {
        //Calls the manager to add a score and give a powerup before destroying the object
        GameObject.FindGameObjectWithTag("Scripts").GetComponent<Manager>().addScore();
        GameObject.FindGameObjectWithTag("Scripts").GetComponent<Manager>().givePowerup();
        Destroy(this.gameObject);
    }

}
