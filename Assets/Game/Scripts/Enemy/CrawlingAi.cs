using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingAi : Enemy {
    public float GrabbingDistance = 2;

    public bool IsGrabbingPlayer = false;
    public Vector3 PlayerOffset = new Vector3(0, 0, 0);

    protected override void UpdateAnimations() {
        animator.SetFloat("Blend", agent.velocity.normalized.magnitude);
    }

    protected override void UpdateFindingPath() {
        if (target != null) {
            if (!IsGrabbingPlayer) {
                if (CheckIfPlayerSeesMe()) {
                    agent.ResetPath();
                }
                else {
                    agent.SetDestination(target.position);
                }
                if ((transform.position - target.position).magnitude < GrabbingDistance && CheckIfPlayerSeesMe()) {
                    IsGrabbingPlayer = true;
                    PlayerOffset = transform.position - target.position;
                    PlayerScript.player.BeGrabbed();
                }
            }
            else {
                transform.position = target.position;
                if (target != null) {
                    if (CheckIfPlayerSeesMe()) {
                        agent.ResetPath();
                    }
                    else if (target != null) {
                        agent.SetDestination(target.position);
                    }
                }
            }
        }
        /*TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;*/
    }

    protected override void Attack() {
        /*if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;*/
            //PlayerScript.BeAttacked();
        //}
    }

    public override void OnDeath() {
        PlayerScript.player.DecrementGrabbers();
        base.OnDeath();
    }

    private bool CheckIfPlayerSeesMe() {
        if(target != null) {
            return target.GetComponent<FirstPersonPlayerController>().CanSee(GetComponent<BoxCollider>());
        }
        return false;
    }
}
