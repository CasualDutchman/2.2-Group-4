using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitHit : MonoBehaviour {

    public GameObject stainObj;

	void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Player")) {
            if (col.gameObject.GetComponent<Player>()) {
                Player player = col.gameObject.GetComponent<Player>();
                player.Hurt(10);
            }
        }

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            Instantiate(stainObj, hit.point, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
