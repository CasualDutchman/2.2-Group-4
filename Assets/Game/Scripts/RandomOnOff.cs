using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomOnOff : MonoBehaviour {

    Light lightObj;

    float randomTime;

    float timer;

	void Start () {
        lightObj = GetComponent<Light>();
        lightObj.enabled = Random.Range(0, 3) == 0;

        randomTime = Random.value / 3f;
	}
	
	void Update () {
        timer += Time.deltaTime;

        if (timer >= randomTime) {
            lightObj.enabled = !lightObj.enabled;

            lightObj.intensity = Random.value / 2f;

            randomTime = !lightObj.enabled ? Random.value * 3f : Random.value / 2f;
            timer = 0;
        }
	}
}
