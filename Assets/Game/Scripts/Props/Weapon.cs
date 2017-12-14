using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    public enum FireMode { Semi, Burst, Auto, ShotGun }

    public string weaponName;

    public PlayerWeaponController.WeaponSlot preveredSlot;

    public FireMode fireMode;
    public float rateOfFire = 1;
    public int maxAmmoMagazine;
    public int ammo;
    public int holdingmaxAmmo;

    public GameObject bulletShell;
}
