using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Pieter

//The ragdoll manager manages all the components that need to change when the enemy goes into ragdoll
public class RagdollManager : MonoBehaviour {

    public Animator animator;

    //All the components that need to change
    public GameObject[] ragdollComponents;

	public void EnableRagdoll() {
        animator.enabled = false;
        for (int i = 0; i < ragdollComponents.Length; i++) {
            ragdollComponents[i].layer = LayerMask.NameToLayer("Water"); //Change the layer to Water, because the player does not interact with that when walking

            if (ragdollComponents[i].GetComponent<Rigidbody>())
                ragdollComponents[i].GetComponent<Rigidbody>().useGravity = true;

            if(ragdollComponents[i].GetComponent<Collider>())
                ragdollComponents[i].GetComponent<Collider>().isTrigger = false;

            if (ragdollComponents[i].GetComponent<EnemyPart>()) {
                Destroy(ragdollComponents[i].GetComponent<EnemyPart>());
            }
        }
    }
}
