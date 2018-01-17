using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHit : MonoBehaviour {

    public GameObject[] objectSpawn;

	void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Enemy")) {
            if (col.gameObject.GetComponent<Enemy>()) {
                Enemy enemy = col.gameObject.GetComponent<Enemy>();
                enemy.Hurt(10);
            }
        }

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            for (int i = 0; i < objectSpawn.Length; i++) {
                Instantiate(objectSpawn[i], hit.point, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}
