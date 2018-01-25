using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: Cristina // Modified: Pieter
public class CrawlingAi : Enemy {

    //Distance when the grabbing is in effect
    public float GrabbingDistance = 2;

    //is he currently holding on to the player?
    public bool IsGrabbingPlayer = false;

    //Make the animation go when he is walking.
    //Stand still when he is not walking
    protected override void UpdateAnimations() {
        animator.SetFloat("Blend", agent.velocity.normalized.magnitude);
    }

    //update path specific to the crawler
    protected override void UpdateFindingPath() {
        if (target != null) {
            if (!IsGrabbingPlayer) {//When he is not holding the player
                if (CheckIfPlayerSeesMe()) {
                    agent.ResetPath();
                } else {
                    agent.SetDestination(target.position);
                }
                //If he is close to the player, hold on
                if ((transform.position - target.position).magnitude < GrabbingDistance) {
                    IsGrabbingPlayer = true;
                    target.GetComponent<Player>().BeGrabbed();
                }
            } else {
                agent.SetDestination(target.position);
            }
        }
    }

    //When he dies, the player needs to be freed
    public override void OnDeath() {
        if (target != null) {
            target.GetComponent<Player>().DecrementGrabbers();
        }
        base.OnDeath();
    }

    //Check if the player can see him
    private bool CheckIfPlayerSeesMe() {
        if(target != null) {
            return target.GetComponent<FirstPersonPlayerController>().CanSee(GetComponent<BoxCollider>());
        }
        return false;
    }

    //Crawler does not attack, but it needs to inherint this function
    protected override void Attack() {}
}
