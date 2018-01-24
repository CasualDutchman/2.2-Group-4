using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {

    public Vector3 angleSpeed;

	void Update () {
        transform.Rotate(angleSpeed * Time.deltaTime, Space.Self);
	}
}
