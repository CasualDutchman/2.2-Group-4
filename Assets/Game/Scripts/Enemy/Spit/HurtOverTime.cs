using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtOverTime : MonoBehaviour {

    public float damage = 2;
    public float interval = 1;

    float timer;

    void OnTriggerStay(Collider col) {
        timer += Time.fixedDeltaTime;
        if (timer >= interval) {
            timer -= interval;

            if (col.CompareTag("Player")) {
                if (col.GetComponent<Player>()) {
                    Player player = col.GetComponent<Player>();
                    player.Hurt(damage);
                }
            }
        }
    }
}
