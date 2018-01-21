using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : Item {

    public float healthToGive = 10;
    public bool oneTimeUse = false;

    public override void Interact(Player player) {
        player.health = Mathf.Clamp(player.health + healthToGive, 0, player.maxHealth);
        player.UpdateHealthText();

        if (oneTimeUse) {
            Destroy(gameObject);
        }
    }

    public override string Message() {
        return "Get Health";
    }
}
