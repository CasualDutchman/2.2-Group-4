using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentGoTo : Enemy {
	void FixedUpdate () {
        //if(!agent.hasPath)
        if (!CanFollowPlayer) {
            DetectPlayerLineOfSight();
        } else {
            if ((transform.position - target.transform.position).magnitude < 1.5f) {
                agent.SetDestination(transform.position);
                Attack();
            } else {
                agent.SetDestination(target.position);
            }
        }
        UpdateLastAttackTime();
    }

    protected override void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;
            PlayerScript.BeAttacked();
        }
    }
}
