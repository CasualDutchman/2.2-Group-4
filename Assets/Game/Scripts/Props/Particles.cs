using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class Particles : MonoBehaviour {

    //When the particle system on the object is stopped, delete the object
    public ParticleSystem system;

    void Update () {
        if (system.isStopped)
            Destroy(gameObject);
	}
}
