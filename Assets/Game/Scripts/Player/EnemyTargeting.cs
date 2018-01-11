using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTargeting : MonoBehaviour {

    public float normalSize = 4;
    public float crouchedSize = 2;
    public float sprintSize = 10;
    public float shootSize = 30;

    FirstPersonPlayerController movementController;
    PlayerWeaponController weaponController;

    Vector3 halfExtents;

    void Start () {
        movementController = GetComponent<FirstPersonPlayerController>();
        weaponController = GetComponent<PlayerWeaponController>();
    }
	
	void Update () {
        halfExtents = new Vector3(normalSize / 2f, 1, normalSize / 2f);

        if (movementController.crouched) {
            halfExtents.x = crouchedSize / 2f;
            halfExtents.z = crouchedSize / 2f;
        }
        else if (movementController.sprinting) {
            halfExtents.x = sprintSize / 2f;
            halfExtents.z = sprintSize / 2f;
        } 

        if(weaponController.shooting) {
            halfExtents.x = shootSize / 2f;
            halfExtents.z = shootSize / 2f;
        }

        Collider[] colliders = Physics.OverlapBox(transform.position + Vector3.up, halfExtents, transform.rotation);
        foreach (Collider hit in colliders) {
            if (hit.GetComponent<Enemy>() && hit.GetComponent<Enemy>().target == null) {
                hit.GetComponent<Enemy>().target = transform;
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireCube(transform.position + Vector3.up, halfExtents * 2);
    }
}
