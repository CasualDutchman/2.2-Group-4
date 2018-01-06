using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour {

    public Transform target;
    protected FirstPersonPlayerController PlayerScript;

    protected bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 1.0f;
    protected float TimeSinceLastAttack = 1.1f;

    protected NavMeshAgent agent;

    // Use this for initialization
    protected virtual void Start () {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerScript = target.GetComponent<FirstPersonPlayerController>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    protected void UpdateLastAttackTime() {
        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    protected void DetectPlayerLineOfSight() {
        RaycastHit OutHit;
        if (Physics.Linecast(transform.position, target.transform.position, out OutHit)) {
            if (OutHit.collider.gameObject.tag == "Player") {
                CanFollowPlayer = true;
            }
        }
    }

    protected abstract void Attack();
}
