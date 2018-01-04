using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTest : MonoBehaviour {

    public Transform target;

    NavMeshAgent agent;
    Animator animator;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
	}
	
	void Update () {
        if (Vector3.Distance(transform.position, target.position) < 10) {
            agent.SetDestination(target.position);
        } else {
            agent.ResetPath();
        }

        animator.SetFloat("Blend", agent.velocity.normalized.magnitude);
	}
}
