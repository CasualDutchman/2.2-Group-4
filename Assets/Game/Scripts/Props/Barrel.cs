using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrel : MonoBehaviour {

    public GameObject soundObj;
    public AudioClip explosionSound;
    public UnityEngine.Audio.AudioMixerGroup effectsMixerGroup;

    public GameObject system;

    public float health = 1;

    bool exploded = false;

	public void Explode() {
        if (!exploded) { //stackoverflow fix
            exploded = true;

            GameObject shotSoundobj = Instantiate(soundObj, transform.position, Quaternion.identity);
            AudioSource source = shotSoundobj.GetComponent<AudioSource>();

            source.outputAudioMixerGroup = effectsMixerGroup;
            source.clip = explosionSound;
            source.Play();

            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, 5);
            foreach (Collider hit in colliders) {

                if (hit.GetComponent<Enemy>()) {
                    print("ENEMY");
                    hit.GetComponent<Enemy>().health -= 100;
                    hit.GetComponent<Enemy>().OnDeath();
                }

                if (hit.GetComponent<Rigidbody>())
                    hit.GetComponent<Rigidbody>().AddExplosionForce(15, explosionPos, 10, 1, ForceMode.Impulse);

                if (hit.gameObject != gameObject && hit.GetComponent<Barrel>()) {
                    hit.GetComponent<Barrel>().Explode();
                    continue;
                }
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
