using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAudio : MonoBehaviour {

    [SerializeField]
    private AudioClip soundClip;
    [SerializeField]
    private AudioSource soundSource;

    void Start ()
    {
        //Sets sound clip and plays at beginning
        soundSource.clip = soundClip;
        soundSource.Play();
    }

    //Scales pitch of sound based on pitch input
    public void setPropAudio(float pitch)
    {
        if(pitch > 0.05)
        {
            soundSource.volume = 1;
            soundSource.pitch = pitch;
        }
        if(pitch < 0.05)
        {
            soundSource.volume = 0;
        }
    }

}
