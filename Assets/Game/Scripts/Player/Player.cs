using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Author: Pieter
public class Player : MonoBehaviour {

    public enum playertype { PlayerOne, PlayerTwo }
    public enum Controltype { Mouse, ControllerOne, ControllerTwo }

    public playertype playerType;//What player is it
    public Controltype controlType;//What controls does this player use

    //attached hud object
    public GameObject hudElement;

    //On death objects
    public GameObject deathRetry, deathExit;
    public Button buttonRetry, buttonExit, buttonExit2;

    //audio information
    public AudioClip[] grunts;
    public AudioClip muchDamage, muchRadiation;
    public bool damgeAudio, damageRadiation;
    public GameObject emptyAudio;

    //health information
    public Image healthImage;
    public float health = 100;
    public float maxHealth = 100;

    //radiation information
    public Image radiationImage;
    public float radiation = 0;
    public float maxRadiation = 100;

    public int lives = 1;

    private int Grabbers = 0;

    void Start () {
        UpdateBars();
    }
	
    //Add a grabber when grabbed by a crawler
	public void BeGrabbed() {
        Grabbers++;
    }

    //remove a grabber when the attached crawler dies
    public void DecrementGrabbers() {
        Grabbers--;
        if (Grabbers < 0) Grabbers = 0;
    }

    public int GetGrabbers() {
        return Grabbers;
    }

    //When hurting the player
    public void Hurt(float amount) {
        health -= amount * DemoScript.instance.enemyDamageMultiplier;//remove from health

        GetMovementController.yaw += Random.Range(0, 10); // add movement to the camera for moving
        GetMovementController.pitch += Random.Range(0, 10);

        UpdateBars();

        GameObject go = Instantiate(emptyAudio, transform.position, Quaternion.identity); //Add grunting audio
        go.transform.parent = transform;
        AudioSource source = go.GetComponent<AudioSource>();
        source.clip = grunts[Random.Range(0, grunts.Length)];
        source.Play();

        if (health <= 40 && !damgeAudio) {//Play soudn when damage is too low
            go = Instantiate(emptyAudio, transform.position, Quaternion.identity);
            go.transform.parent = transform;
            source = go.GetComponent<AudioSource>();
            source.clip = muchDamage;
            source.volume = 0.5f;
            source.Play();
            damgeAudio = true;
        }

        if (health <= 0) //die when no health
            OnDeath();
    }

    public void Radiate(float amount) {
        radiation += amount;

        UpdateBars();

        if (radiation >= 80 && !damageRadiation) {//play sound when ratiation is too low
            GameObject go = Instantiate(emptyAudio, transform.position, Quaternion.identity);
            go.transform.parent = transform;
            AudioSource source = go.GetComponent<AudioSource>();
            source.clip = muchRadiation;
            source.Play();
            damageRadiation = true;
        }

        if (radiation >= 100) // die when full ratiation
            OnDeath();
    }

    public void UpdateBars() { //update the health and radiation bar when needed
        healthImage.fillAmount = health / maxHealth;
        radiationImage.fillAmount = radiation / maxRadiation;
    }

    //When the player dies
    public void OnDeath() {
        lives--;//remove a live

        //show death screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (lives <= 0) {
            deathExit.SetActive(true);
            buttonExit2.onClick.AddListener(() => OnExit());
            return;
        }

        deathRetry.SetActive(true);
        buttonRetry.onClick.AddListener(() => OnRetry());
        buttonExit.onClick.AddListener(() => OnExit());

        Time.timeScale = 0;
    }

    //When the death screen's retry button is pressed
    public void OnRetry() {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GridWorld.instance.OnPlayerDeath(this, lives);
    }

    //When the death screen's quit button is pressed
    public void OnExit() {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
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
