using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPart : MonoBehaviour {

    public Enemy connected;

    public float hitMultiplier;

	public void Damage(float damage) {
        if (connected.health > 0) {
            connected.health -= damage * hitMultiplier;

            if (connected.health <= 0) {
                connected.OnDeath();
            }
        }
    }

    void OnParticleCollision(GameObject other) {
        if (connected.health > 0) {
            connected.health -= 1;

            if (connected.health <= 0) {
                connected.OnDeath();
            }
        }
    }
}
