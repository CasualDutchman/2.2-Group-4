using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour {

    public enum WeaponSlot { primary, secondary }

    public Weapon currentWeapon;
    public WeaponSlot currentSlot = WeaponSlot.primary;

    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    FirstPersonPlayerController control;

	void Start () {
        control = GetComponent<FirstPersonPlayerController>();
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwapWeapon(WeaponSlot.primary);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwapWeapon(WeaponSlot.secondary);
        }
    }

    void SwapWeapon(WeaponSlot slot) {
        if (currentSlot == slot)
            return;

        if(slot == WeaponSlot.primary && primaryWeapon != null) {
            currentSlot = slot;
            DrawWeapon(primaryWeapon);
        }

        if (slot == WeaponSlot.secondary && secondaryWeapon != null) {
            currentSlot = slot;
            DrawWeapon(secondaryWeapon);
        }
    }

    void DrawWeapon(Weapon weapon) {
        currentWeapon = weapon;

        control.MaxAmmo = weapon.maxAmmo;
        control.Ammo = weapon.maxAmmo;
    }
}
