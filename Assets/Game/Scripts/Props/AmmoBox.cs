using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : Item {

    public bool oneTimeUse = false;

    public override void Interact(Player player) {
        PlayerWeaponController weaponcontroller = player.GetWeaponController;
        if (weaponcontroller.currentWeapon != null) {
            weaponcontroller.currentWeapon.holdingmaxAmmo = weaponcontroller.currentWeapon.maxAmmoMagazine * 5;
            weaponcontroller.UpdateAmmoCounter();

            if (oneTimeUse) {
                Destroy(gameObject);
            }
        }
    }

    public override string Message() {
        return "Get Ammo";
    }
}
