using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingAi : Enemy {
    private GameObject PlayerCamera;
    
    protected override void Start() {
        base.Start();
        PlayerCamera = GameObject.FindGameObjectWithTag("PlayerCamera");
    }

	void FixedUpdate () {
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
        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    protected override void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;
            PlayerScript.BeAttacked();
        }
    }

    private bool CheckIfPlayerSeesMe() {
        Vector3 PlayerLookDirection = PlayerCamera.transform.forward;
        Transform PlayerLookAtMeTransform = PlayerCamera.transform;
        PlayerLookAtMeTransform.LookAt(transform);
        return Vector3.Dot(PlayerLookDirection, PlayerLookAtMeTransform.forward) > 0.55;
    }
}
