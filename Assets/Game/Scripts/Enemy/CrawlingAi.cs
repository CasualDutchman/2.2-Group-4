using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingAi : Enemy {
    private GameObject PlayerCamera;
    
    protected override void Start() {
        base.Start();
        //PlayerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
    }

    protected override void UpdateFindingPath() {
        /*
        if (!CheckIfPlayerSeesMe()) {
            if (!CanFollowPlayer) {
                RaycastHit OutHit;
                if (Physics.Linecast(transform.position, target.transform.position, out OutHit)) {
                    if (OutHit.collider.gameObject.tag == "Player") {
                        CanFollowPlayer = true;
                    }
                }
            }
            else {
                if ((transform.position - target.transform.position).magnitude < 1.5f) {
                    agent.SetDestination(transform.position);
                    Attack();
                }
                else {
                    agent.SetDestination(target.position);
                }
            }

        } else {
            agent.SetDestination(transform.position);
        }
        */
        if(target != null) {
            if (CheckIfPlayerSeesMe()) {
                agent.ResetPath();
            }else {
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
