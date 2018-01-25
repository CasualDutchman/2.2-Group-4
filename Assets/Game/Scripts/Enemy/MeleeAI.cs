using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Author: Cristina // Modified: Pieter
public class MeleeAI : Enemy {

    //The amount of damage done to the player, when hit
    public float damageDone = 10;

    protected override void OnStart() {
        SetSpeed(Random.Range(1.5f, 5.0f)); // assign a random speed
    }

    protected override void UpdateFindingPath() {
        base.UpdateFindingPath();

        //When a melee enemy with a higher speed is close, set to that speed
        Vector3 halfExtents = new Vector3(2 / 2f, 1, 2 / 2f);
        Collider[] colliders = Physics.OverlapBox(transform.position + Vector3.up, halfExtents, transform.rotation);
        foreach (Collider hit in colliders) {
            if (hit.GetComponent<EnemyPart>() && hit.GetComponent<EnemyPart>().connected is MeleeAI) {
                if (hit.GetComponent<EnemyPart>().connected.GetComponent<NavMeshAgent>().speed > GetComponent<NavMeshAgent>().speed) {
                    SetSpeed(hit.GetComponent<EnemyPart>().connected.GetComponent<NavMeshAgent>().speed);
                }
            }
        }
    }

    protected override void UpdateAnimations() {
        animator.SetFloat("Blend", agent.velocity.normalized.magnitude); // go to the walking animation when moving, otherwise just do idle

        if (isAttacking) {//attacking animation
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95) {
                    isAttacking = false;
                }
            }
        } 
    }

    //attack check, called every frame
    protected override void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;

            if (!isAttacking) {
                StartCoroutine(AttackAnimation());
                isAttacking = true;
            }
        }
    }

    //Animation call and sound call
    IEnumerator AttackAnimation() {
        animator.SetTrigger("Attack1");

        source.clip = attackAudio;
        source.Play();

        yield return new WaitForSeconds(0.4f);
        HurtWhenInRange();
    }

    //When the player is hit by the enemy
    void HurtWhenInRange() {
        if ((transform.position - target.transform.position).magnitude < 2f) {
            target.GetComponent<Player>().Hurt(damageDone);
            target.GetComponent<Player>().Radiate(damageDone * 0.5f);
        }
    }
}
