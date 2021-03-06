﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float BulletSpeed = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position += transform.forward * Time.deltaTime * BulletSpeed;
	}

    void OnTriggerEnter(Collider collision) {
        if (collision.CompareTag("Enemy")) {
            Destroy(collision.gameObject);
        }
        Destroy(gameObject);
    }
}
