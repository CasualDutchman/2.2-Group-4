using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public Text healthText;
    public float health = 100;
    public float maxHealth = 100;

	void Start () {
        healthText.text = health + "/" + maxHealth + "";
    }
	
	void Update () {
		
	}

    public void Hurt() {
        health -= 10;

        healthText.text = health + "/" + maxHealth + "";
    }
}
