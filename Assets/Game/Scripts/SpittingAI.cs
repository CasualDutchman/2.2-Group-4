using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpittingAI : Enemy {
    public float SpittingDistance = 5.0f;
    public float SpittingForce = 1000.0f;

    public Transform SpitClass;

    void FixedUpdate() {
        //if(!agent.hasPath)
        if (!CanFollowPlayer) {
            DetectPlayerLineOfSight();
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
        UpdateLastAttackTime();
    }

    protected override void Attack() {
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
