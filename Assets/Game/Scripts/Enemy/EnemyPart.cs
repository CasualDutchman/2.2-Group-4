using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPart : MonoBehaviour {

    public Enemy connected;

    public float hitMultiplier;

	public void Damage(float damage) {
        connected.health -= damage * hitMultiplier;
    }
}
