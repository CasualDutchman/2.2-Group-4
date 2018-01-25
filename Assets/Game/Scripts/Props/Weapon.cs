using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class Weapon : Item {

    //What is the mode of the weapon
    public enum FireMode { Semi, Burst, Auto, ShotGun, Flamethrower, Melee, Throwable }

    //name of the weapon
    public string weaponName;

    //What slot of the weapon controller the weapon will occupie
    public PlayerWeaponController.WeaponSlot preveredSlot;

    [HideInInspector]
    public AudioSource audioSource;

    //audioclips per state
    public AudioClip shoot, begin, end;

    //Where the left hand goes
    public Transform secondHand;

    //properties
    public FireMode fireMode;
    public float rateOfFire = 1;
    public float affectedByRecoilFactor = 1;
    public int maxAmmoMagazine;
    public int ammo;
    public int holdingmaxAmmo;
    public float reloadTime;
    public float damageDone = 10;

    //what bulletshell to eject, after shooting
    public GameObject bulletShell;

    //Where to shoot from and where to spawn the muzzleflash
    [HideInInspector]
    public Transform muzzle;

    public GameObject muzzleFlash;

    public GameObject throwableObj;

    void OnEnable() {
        muzzle = transform.Find("Muzzle");
        audioSource = GetComponent<AudioSource>();
    }

    //What to display when hover over
    public override string Message() {
        return "Pick up " + weaponName;
    }

    //What happens when you press the interact button
    public override void Interact(Player player) {
        PlayerWeaponController weaponcontroller = player.GetWeaponController;
        weaponcontroller.PickUpGun(this);
    }

    //If this is a molotov cocktail and it is on the ground, explode
    void OnParticleCollision(GameObject other) {
        if (fireMode == Weapon.FireMode.Throwable && weaponName.ToLower().StartsWith("molo")) {
            GameObject go = Instantiate(throwableObj);
            go.transform.position = transform.position;

            Destroy(gameObject);
        }
    }
}
