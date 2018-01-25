using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter
public class RandomPlacing : MonoBehaviour {

    //Spawn locations for objects
    public Transform[] spawnLocations;

    //objects to choose from
    public GameObject[] spawnables;

    //change meter
    public Vector2 changeIn = new Vector2(2, 4);

	void Start () {
        //Spawn an object on the spawnlocation, if the random number generator allow for it
        for (int i = 0; i < spawnLocations.Length; i++) {
            if (Random.Range(0, changeIn.y) < changeIn.x) {
                Transform loc = spawnLocations[i];

                GameObject go = Instantiate(spawnables[Random.Range(0, spawnables.Length)], loc.position, loc.rotation);

                go.transform.parent = loc;
                go.transform.position = loc.position + Vector3.up * 0.1f;

                if (go.GetComponent<Weapon>()) {
                    Weapon weapon = go.GetComponent<Weapon>();
                    weapon.ammo = Random.Range(2, weapon.maxAmmoMagazine);
                    weapon.holdingmaxAmmo = Random.Range(0, weapon.maxAmmoMagazine);
                }
            }
        }
	}
}
