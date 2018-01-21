using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingAi : Enemy {

    protected override void UpdateAnimations() {
        animator.SetFloat("Blend", agent.velocity.normalized.magnitude);
    }

    protected override void UpdateFindingPath() {
        if(target != null) {
            if (CheckIfPlayerSeesMe()) {
                agent.ResetPath();
            }else if(target != null) {
                agent.SetDestination(target.position);
            }
        }

        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    protected override void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;
            //PlayerScript.BeAttacked();
        }
    }

    private bool CheckIfPlayerSeesMe() {
        if(target != null) {
            return target.GetComponent<FirstPersonPlayerController>().CanSee(GetComponent<BoxCollider>());
        }
        return false;
    }
}
