using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter

//Enemy part is part of colliders on limbs of the enemy
//This allows for multipliers at certain limbs (headshot would do more damage then a toe)
public class EnemyPart : MonoBehaviour {

    //The enemy this is connected to
    public Enemy connected;

    //When the enemy is hit, this multiplier allows for differences
    public float hitMultiplier;

    //called in the shooting class, calls when hitting the enemy
	public void Damage(float damage, Transform shooter) {
        if (connected.health > 0) {
            connected.Hurt(damage * hitMultiplier);

            if (connected.target == null) {
                connected.target = shooter;
            }
        }
    }

    //When a fire particle is hitting the enemy
    void OnParticleCollision(GameObject other) {
        if (connected.health > 0) {
            connected.Hurt(0.25f);
            connected.SetFire();
        }
    }
}
