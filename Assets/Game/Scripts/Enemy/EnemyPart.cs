using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPart : MonoBehaviour {

    public Enemy connected;

    public float hitMultiplier;

	public void Damage(float damage, Transform shooter) {
        if (connected.health > 0) {
            connected.Hurt(3 * hitMultiplier);

            if (connected.target == null) {
                connected.target = shooter;
            }
        }
    }

    void OnParticleCollision(GameObject other) {
        if (connected.health > 0) {
            connected.Hurt(10);
        }
    }
}
