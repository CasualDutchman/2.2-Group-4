using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class AmmoBox : Item {

    public bool oneTimeUse = false;

    public int bulletAmount = 0;

    //Give ammo when interacting with it
    public override void Interact(Player player) {
        PlayerWeaponController weaponcontroller = player.GetWeaponController;
        if (weaponcontroller.currentWeapon != null) {
            if (bulletAmount <= 0) {
                weaponcontroller.currentWeapon.holdingmaxAmmo = weaponcontroller.currentWeapon.maxAmmoMagazine * 5;
            } else {
                weaponcontroller.currentWeapon.holdingmaxAmmo += bulletAmount;
            }
            weaponcontroller.UpdateAmmoCounter();

            if (oneTimeUse) {
                Destroy(gameObject);
            }
        }
    }

    //message
    public override string Message() {
        return "Get Ammo" + (bulletAmount <= 0 ? "" : "(" +  bulletAmount.ToString() + ")") ;
    }
}
