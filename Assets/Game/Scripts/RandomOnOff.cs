using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomOnOff : MonoBehaviour {

    Light light;

    float randomTime;

    float timer;

	void Start () {
        light = GetComponent<Light>();
        light.enabled = Random.Range(0, 3) == 0;

        randomTime = Random.value / 3f;
	}
	
	void Update () {
        timer += Time.deltaTime;

        if (timer >= randomTime) {
            light.enabled = !light.enabled;

            light.intensity = Random.value / 2f;

            randomTime = !light.enabled ? Random.value * 3f : Random.value / 2f;
            timer = 0;
        }
	}
}
