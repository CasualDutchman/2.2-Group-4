using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentGoTo : MonoBehaviour {

    public Transform target;
    Player player;

    private bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 1.0f;
    private float TimeSinceLastAttack = 1.1f;

    NavMeshAgent agent;

    public float health = 100f;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        
        target = GameObject.FindGameObjectWithTag("Player").transform;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        //PlayerScript = target.GetComponent<FirstPersonPlayerController>();
    }

    void OnParticleCollision(GameObject other) {
        health -= 2;

        if (health <= 0) {
            Destroy(gameObject);
        }
    }

    void FixedUpdate () {

        if (!CanFollowPlayer && target != null) {
            RaycastHit OutHit;
            if (Physics.Linecast(transform.position, target.transform.position, out OutHit)) {
                if (OutHit.collider.gameObject.tag == "Player") {
                    CanFollowPlayer = true;
                }
            }
        } else if(target != null){
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
            player.Hurt();
        }
    }
}
