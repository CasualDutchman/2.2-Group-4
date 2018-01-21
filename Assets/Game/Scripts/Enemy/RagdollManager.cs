using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour {

    public Animator animator;
    public GameObject[] ragdollComponents;

	public void EnableRagdoll() {
        animator.enabled = false;
        for (int i = 0; i < ragdollComponents.Length; i++) {
            ragdollComponents[i].layer = LayerMask.NameToLayer("Water");

            if (ragdollComponents[i].GetComponent<Rigidbody>())
                ragdollComponents[i].GetComponent<Rigidbody>().useGravity = true;

            if(ragdollComponents[i].GetComponent<Collider>())
                ragdollComponents[i].GetComponent<Collider>().isTrigger = false;

            if (ragdollComponents[i].GetComponent<EnemyPart>()) {
                Destroy(ragdollComponents[i].GetComponent<EnemyPart>());
            }
        }

        //Destroy(this);
    }
}
