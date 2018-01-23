using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitHit : MonoBehaviour {

    public GameObject stainObj;

	void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Player")) { // When spit hits a player, hurt the player directely
            if (col.gameObject.GetComponent<Player>()) {
                Player player = col.gameObject.GetComponent<Player>();
                player.Hurt(10);
            }
        }

        Ray ray = new Ray(transform.position, Vector3.down); // spawn an acid stain on the ground
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            int ymultiplier = Mathf.FloorToInt(hit.point.y / 2.5f);
            Instantiate(stainObj, new Vector3(hit.point.x, ymultiplier * 2.5f, hit.point.z), Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
