using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudio : MonoBehaviour
{

    [SerializeField]
    private AudioClip musicClip;
    [SerializeField]
    private AudioSource musicSource;

	void Start ()
    {
        //Sets music clip and plays music at beginning
        musicSource.clip = musicClip;
        musicSource.Play();
	}

}
