using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

    public enum playertype { PlayerOne, PlayerTwo }
    public enum Controltype { Mouse, ControllerOne, ControllerTwo }

    public playertype playerType;
    public Controltype controlType;

    public GameObject hudElement;

    public GameObject deathRetry, deathExit;
    public Button buttonRetry, buttonExit, buttonExit2;

    public AudioClip[] grunts;
    public AudioClip muchDamage, muchRadiation;
    public bool damgeAudio, damageRadiation;
    public GameObject emptyAudio;

    public Image healthImage;
    public float health = 100;
    public float maxHealth = 100;

    public Image radiationImage;
    public float radiation = 0;
    public float maxRadiation = 100;

    public int lives = 1;

    private int Grabbers = 0;

    void Start () {
        UpdateBars();
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

    public void Hurt(float amount) {
        health -= amount * DemoScript.instance.enemyDamageMultiplier;

        GetMovementController.yaw += Random.Range(0, 10);
        GetMovementController.pitch += Random.Range(0, 10);

        UpdateBars();

        GameObject go = Instantiate(emptyAudio, transform.position, Quaternion.identity);
        go.transform.parent = transform;
        AudioSource source = go.GetComponent<AudioSource>();
        source.clip = grunts[Random.Range(0, grunts.Length)];
        source.Play();

        if (health <= 40 && !damgeAudio) {
            go = Instantiate(emptyAudio, transform.position, Quaternion.identity);
            go.transform.parent = transform;
            source = go.GetComponent<AudioSource>();
            source.clip = muchDamage;
            source.volume = 0.5f;
            source.Play();
            damgeAudio = true;
        }

        if (health <= 0)
            OnDeath();
    }

    public void Radiate(float amount) {
        radiation += amount;

        UpdateBars();

        if (radiation >= 80 && !damageRadiation) {
            GameObject go = Instantiate(emptyAudio, transform.position, Quaternion.identity);
            go.transform.parent = transform;
            AudioSource source = go.GetComponent<AudioSource>();
            source.clip = muchRadiation;
            source.Play();
            damageRadiation = true;
        }

        if (radiation >= 100)
            OnDeath();
    }

    public void UpdateBars() {
        healthImage.fillAmount = health / maxHealth;
        radiationImage.fillAmount = radiation / maxRadiation;
    }

    public void OnDeath() {
        lives--;

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

    public void OnRetry() {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GridWorld.instance.OnPlayerDeath(this, lives);
    }

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
