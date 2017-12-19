using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour {

    public GameObject system;

    public float health = 1;

    bool exploded = false;

	public void Explode() {
        if (!exploded) { //stackoverflow fix
            exploded = true;
            
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, 5);
            foreach (Collider hit in colliders) {
                if (hit.gameObject != gameObject && hit.GetComponent<Barrel>()) {
                    hit.GetComponent<Barrel>().Explode();
                    continue;
                }
                if (hit.GetComponent<Rigidbody>())
                    hit.GetComponent<Rigidbody>().AddExplosionForce(15, explosionPos, 10, 1, ForceMode.Impulse);
            }
            Instantiate(system, explosionPos, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void OnParticleCollision(GameObject other) {
        health -= 0.1f;

        if (health <= 0) {
            Explode();
        }
    }
}
