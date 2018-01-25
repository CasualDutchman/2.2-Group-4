using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class SoundDelete : MonoBehaviour {

    //This class is used to make different sound objects.
    //This way sounds won't cut eachother off, when a new one needs to be played

    AudioSource source;

	void Start () {
        source = GetComponent<AudioSource>();
        source.Play();
    }
	
    //Destroy sound object when the sound is done playing
	void Update () {
        if (!source.isPlaying) {
            Destroy(gameObject);
        }
	}
}
