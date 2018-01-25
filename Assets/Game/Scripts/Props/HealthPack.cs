using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class HealthPack : Item {

    public float healthToGive = 10;
    public bool oneTimeUse = false; 

    //Give health to the player when interacting
    public override void Interact(Player player) {
        player.health = Mathf.Clamp(player.health + healthToGive, 0, player.maxHealth);
        player.UpdateBars();

        if (oneTimeUse) {
            Destroy(gameObject);
        }
    }

    //message
    public override string Message() {
        return "Get Health";
    }
}
