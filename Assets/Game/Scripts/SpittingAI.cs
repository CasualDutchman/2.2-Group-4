using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpittingAI : MonoBehaviour {

    public Transform target;
    private FirstPersonPlayerController PlayerScript;

    public float SpittingDistance = 5.0f;
    public float SpittingForce = 1000.0f;

    private bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 1.0f;
    private float TimeSinceLastAttack = 1.1f;

    public Transform SpitClass;

    NavMeshAgent agent;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerScript = target.GetComponent<FirstPersonPlayerController>();
    }

    void FixedUpdate() {
        //if(!agent.hasPath)
        if (!CanFollowPlayer) {
            RaycastHit OutHit;
            if (Physics.Linecast(transform.position, target.transform.position, out OutHit)) {
                if (OutHit.collider.gameObject.tag == "Player") {
                    CanFollowPlayer = true;
                }
            }
        }
        else {
            if ((transform.position - target.transform.position).magnitude > SpittingDistance) {
                agent.SetDestination(target.position);
            }
            else {
                agent.SetDestination(transform.position);
                transform.LookAt(target.transform);
                Attack();
            }
        }

        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    private void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;
            Transform Spit = Instantiate(SpitClass);
            Spit.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            Spit.transform.LookAt(target);
            Rigidbody SpitRigidBody = Spit.GetComponent<Rigidbody>();
            SpitRigidBody.AddForce(Spit.forward*SpittingForce);
        }
    }
}
