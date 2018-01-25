using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class Spinner : MonoBehaviour {

    //Spin the object to the speed and angle specified
    public Vector3 angleSpeed;

	void Update () {
        transform.Rotate(angleSpeed * Time.deltaTime, Space.Self);
	}
}
