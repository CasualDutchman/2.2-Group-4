﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayerController : MonoBehaviour {

    CharacterController controller;

    public float mouseSpeed = 5;
    private float yaw = 0.0f;
    private float pitch = 0.0f;

    public bool invertY = false;

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    public float DelayBetweenShots = 1.0f;
    private float TimeSinceLastShot = 1.1f;

    public float HalfRecoilAnimationLength = 0.25f;
    public float RecoilEndPitchRotation = -15.0f;
    private float ElapsedRecoilAnimationTime = 0.0f;
    private bool IsRecoiling = false;
    private bool IsReverseRecoilingAnim = false;
    private GameObject Arms;
    private Vector3 RecoilStartingRotation;
    private GameObject Camera;

    public GameObject BulletClass;
    private Transform BulletSpawnPoint;

    public int Ammo = 10;
    public int MaxAmmo = 20;
    public Text AmmoTextArea;
    public string AmmoPrefix = "Ammo: ";

    public int Health = 10;
    public int MaxHealth = 10;
    public Text HealthTextArea;
    public string HealthPrefix = "Health: ";

    void Start () {
        controller = GetComponent<CharacterController>();
        Arms = GameObject.FindGameObjectWithTag("PlayerArms");
        Camera = GameObject.FindGameObjectWithTag("PlayerCamera");
        BulletSpawnPoint = GameObject.FindGameObjectWithTag("PlayerBulletSpawnPoint").transform;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update () {
        //looking around

        //print(Input.GetKeyDown("joystick button 0"));

        //yaw += Input.get * mouseSpeed;
        yaw += Input.GetAxis("Mouse X") * mouseSpeed;
        pitch = Mathf.Clamp(pitch + (Input.GetAxis("Mouse Y") * mouseSpeed * (invertY ? -1 : 1)), -90, 90);

        transform.GetChild(0).localEulerAngles = new Vector3(pitch, 0, 0);
        transform.localEulerAngles = new Vector3(0, yaw, 0);

        if (Input.GetKeyDown(KeyCode.E)) {
            Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if(hit.distance < 10) {
                    if (hit.collider.CompareTag("Door")) {
                        if (hit.collider.GetComponent<Door>()) {
                            hit.collider.GetComponent<Door>().ChangeState();
                        } else {
                            hit.transform.parent.GetComponent<Door>().ChangeState();
                        }
                    }
                }
            }
        }

        //walking around
        if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
        ///////////////////////////////////////////////////////////////////////
        RecoilStartingRotation = Camera.transform.eulerAngles;
        if (Input.GetButton("Fire1")) Shoot();
        if (Input.GetKeyDown("r")) Ammo = MaxAmmo;
        if (IsRecoiling) Recoil();
        TimeSinceLastShot += Time.deltaTime;
        if (TimeSinceLastShot >= 10.0f) TimeSinceLastShot = DelayBetweenShots+1;
        AmmoTextArea.text = AmmoPrefix + Ammo + " / " + MaxAmmo;
        HealthTextArea.text = HealthPrefix + Health + " / " + MaxHealth;
    }

    private void Shoot() {
        if (Ammo > 0 && TimeSinceLastShot >= DelayBetweenShots) {
            IsRecoiling = true;
            RaycastHit OutHit;
            if (Physics.Linecast(Camera.transform.position, Camera.transform.forward * 2000, out OutHit)) {
                SpawnBullet(OutHit.point);
            } else {
                SpawnBullet(Camera.transform.forward * 2000);
            }
            //Debug.DrawLine(Camera.transform.position, OutHit.point, Color.blue, 100.0f);
            TimeSinceLastShot = 0.0f;
            Ammo--;
        }
    }

    private void Recoil() {
        float AnimationPercentage = ElapsedRecoilAnimationTime / HalfRecoilAnimationLength;
        if (!IsReverseRecoilingAnim) {
            Arms.transform.eulerAngles = new Vector3(RecoilStartingRotation.x + (RecoilEndPitchRotation * AnimationPercentage), Arms.transform.eulerAngles.y, Arms.transform.eulerAngles.z);
            ElapsedRecoilAnimationTime += Time.deltaTime;
            if (ElapsedRecoilAnimationTime >= HalfRecoilAnimationLength) {
                IsReverseRecoilingAnim = true;
                ElapsedRecoilAnimationTime = 0.0f;
            }
        } else { // reverse the animation
            Arms.transform.eulerAngles = new Vector3(RecoilStartingRotation.x + (RecoilEndPitchRotation * (1- AnimationPercentage)), Arms.transform.eulerAngles.y, Arms.transform.eulerAngles.z);
            ElapsedRecoilAnimationTime += Time.deltaTime;
            if (ElapsedRecoilAnimationTime >= HalfRecoilAnimationLength) {
                IsReverseRecoilingAnim = false;
                ElapsedRecoilAnimationTime = 0.0f;
                IsRecoiling = false;
            }
        }
    }

    private void SpawnBullet(Vector3 BulletTargetPoint) {
        GameObject Bullet = Instantiate(BulletClass);
        Bullet.transform.position = BulletSpawnPoint.position;
        Bullet.transform.rotation = BulletSpawnPoint.rotation;
        /*
        Transform BulletTargetTransform = new GameObject().transform; // creates an empty game object just to have a new empty transform, that is given a value on the next line
        BulletTargetTransform.position = BulletTargetPoint;
        Bullet.transform.LookAt(BulletTargetTransform);
        */
        //Bullet.transform.rotation = BulletSpawnPoint.rotation;
        
        Transform BulletTargetTransform = new GameObject().transform; // creates an empty game object just to have a new empty transform, that is given a value on the next line
        BulletTargetTransform.position = BulletTargetPoint;
        Bullet.transform.LookAt(BulletTargetTransform);
        
    }

    public void BeAttacked() {
        Health--;
        if (Health <= 0) Destroy(gameObject);
    }
}
