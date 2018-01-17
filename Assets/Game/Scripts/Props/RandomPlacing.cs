using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlacing : MonoBehaviour {

    public Transform[] spawnLocations;
    public GameObject[] spawnables;

    public Vector2 changeIn = new Vector2(2, 4);

    public bool makeParent = false;

	void Start () {
        for (int i = 0; i < spawnLocations.Length; i++) {
            if (Random.Range(0, changeIn.y) < changeIn.x) {
                Transform loc = spawnLocations[i];

                GameObject go = Instantiate(spawnables[Random.Range(0, spawnables.Length)], loc.position, loc.rotation);

                if (makeParent) {
                    go.transform.parent = loc;
                    go.transform.position = loc.position + Vector3.up * 0.1f;
                }

                if (go.GetComponent<Weapon>()) {
                    Weapon weapon = go.GetComponent<Weapon>();
                    weapon.ammo = Random.Range(2, weapon.maxAmmoMagazine);
                    weapon.holdingmaxAmmo = Random.Range(0, weapon.maxAmmoMagazine);
                }
            }
        }
	}
}
