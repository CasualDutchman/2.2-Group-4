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

    private int Grabbers = 0;

	void Start () {
        UpdateHealthText();
    }
	
	void Update () {
		
	}

    public void Hurt(float amount) {
        health -= amount * DemoScript.instance.enemyDamageMultiplier;

        UpdateHealthText();
    }

    public void BeGrabbed() {
        Grabbers++;
    }

    public void DecrementGrabbers() {
        Grabbers--;
        if (Grabbers < 0) Grabbers = 0;
    }

    public int GetGrabbers() {
        return Grabbers;
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
