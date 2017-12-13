using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentGoTo : MonoBehaviour {

    public Transform target;
    private FirstPersonPlayerController PlayerScript;

    private bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 1.0f;
    private float TimeSinceLastAttack = 1.1f;

    NavMeshAgent agent;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        
        target = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerScript = target.GetComponent<FirstPersonPlayerController>();
	}
	
	void FixedUpdate () {

        if (!CanFollowPlayer) {
            RaycastHit OutHit;
            if (Physics.Linecast(transform.position, target.transform.position, out OutHit)) {
                if (OutHit.collider.gameObject.tag == "Player") {
                    CanFollowPlayer = true;
                }
            }
        } else {
            agent.SetDestination(target.position);
            if ((transform.position - target.transform.position).magnitude < 1.5f) {
                Attack();
            }
        }

        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    private void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;
            PlayerScript.BeAttacked();
        }
    }
}
