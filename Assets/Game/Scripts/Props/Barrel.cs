using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class Barrel : MonoBehaviour {

    //Object with particleSystem to spawn
    public GameObject soundObj;

    //Sound information
    public AudioClip explosionSound;
    public UnityEngine.Audio.AudioMixerGroup effectsMixerGroup;

    public GameObject system;

    public float health = 1;

    bool exploded = false;

    //When it explodes
	public void Explode() {
        if (!exploded) { //stackoverflow fix
            exploded = true;

            //add sound
            GameObject shotSoundobj = Instantiate(soundObj, transform.position, Quaternion.identity);
            AudioSource source = shotSoundobj.GetComponent<AudioSource>();

            source.outputAudioMixerGroup = effectsMixerGroup;
            source.clip = explosionSound;
            source.Play();

            //Interact with all the rigidbodys in the area
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, 5);
            foreach (Collider hit in colliders) {

                if (hit.GetComponent<Enemy>()) {
                    hit.GetComponent<Enemy>().Hurt(100);
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

    //When hit with a flamethrower
    void OnParticleCollision(GameObject other) {
        health -= 0.1f;

        if (health <= 0) {
            Explode();
        }
    }
}
