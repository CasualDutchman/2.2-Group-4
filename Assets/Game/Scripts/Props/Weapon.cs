using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item {

    public enum FireMode { Semi, Burst, Auto, ShotGun, Flamethrower, Melee, Throwable }

    public string weaponName;

    public PlayerWeaponController.WeaponSlot preveredSlot;

    [HideInInspector]
    public AudioSource audioSource;

    public AudioClip shoot, begin, end;

    public Transform secondHand;

    public FireMode fireMode;
    public float rateOfFire = 1;
    public float affectedByRecoilFactor = 1;
    public int maxAmmoMagazine;
    public int ammo;
    public int holdingmaxAmmo;
    public float reloadTime;

    public GameObject bulletShell;

    [HideInInspector]
    public Transform muzzle;

    public GameObject muzzleFlash;

    public GameObject throwableObj;

    void OnEnable() {
        muzzle = transform.Find("Muzzle");
        audioSource = GetComponent<AudioSource>();
    }

    public override string Message() {
        return "Pick up " + weaponName;
    }

    public override void Interact(Player player) {
        PlayerWeaponController weaponcontroller = player.GetWeaponController;
        weaponcontroller.PickUpGun(this);
    }

    void OnParticleCollision(GameObject other) {
        if (fireMode == Weapon.FireMode.Throwable && weaponName.ToLower().StartsWith("molo")) {
            GameObject go = Instantiate(throwableObj);
            go.transform.position = transform.position;

            Destroy(gameObject);
        }
    }
}
