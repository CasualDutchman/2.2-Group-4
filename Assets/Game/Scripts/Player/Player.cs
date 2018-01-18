using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public enum playertype { PlayerOne, PlayerTwo }
    public enum Controltype { Mouse, ControllerOne, ControllerTwo }

    public playertype playerType;
    public Controltype controlType;

    public Text healthText;
    public float health = 100;
    public float maxHealth = 100;

	void Start () {
        UpdateHealthText();
    }
	
	void Update () {
		
	}

    public void Hurt(float amount) {
        health -= amount * DemoScript.instance.enemyDamageMultiplier;

        UpdateHealthText();
    }

    public void UpdateHealthText() {
        healthText.text = health + "/" + maxHealth + "";
    }

    public FirstPersonPlayerController GetMovementController {
        get {
            return GetComponent<FirstPersonPlayerController>();
        }
    }

    public PlayerWeaponController GetWeaponController {
        get {
            return GetComponent<PlayerWeaponController>();
        }
    }
}
