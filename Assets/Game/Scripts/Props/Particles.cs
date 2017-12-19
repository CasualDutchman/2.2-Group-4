using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour {

	void Update () {
        if (GetComponent<ParticleSystem>().isStopped)
            Destroy(gameObject);
	}
}
