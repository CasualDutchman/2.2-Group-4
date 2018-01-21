using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour {

    public ParticleSystem system;

    void Update () {
        if (system.isStopped)
            Destroy(gameObject);
	}
}
