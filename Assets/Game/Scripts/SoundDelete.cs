using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDelete : MonoBehaviour {

    AudioSource source;

	void Start () {
        source = GetComponent<AudioSource>();
        source.Play();
    }
	
	void Update () {
        if (!source.isPlaying) {
            Destroy(gameObject);
        }
	}
}
