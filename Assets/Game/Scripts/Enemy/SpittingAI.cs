using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Author: Cristina // Modified: Pieter
public class SpittingAI : Enemy {
    //Distance when to spit acid at the player
    public float SpittingDistance = 5.0f;

    //What object to spit
    public Transform SpitClass;

    //Where to spit the acid from
    public Transform spitBegin;

    protected override void UpdateFindingPath() {
        if(target != null) {
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

    //calculate the spitting velocity according to the player, This results into a predictable arc tot he player
    public static Vector3 CalculateSpitVelocity(Transform target, Vector3 spit) {
        float gravity = Physics.gravity.y;
        float maxheight = Mathf.Max(target.position.y, spit.y);
        float h = maxheight + 0.2f;

        float displacementY = target.position.y - spit.y;
        Vector3 displacementXZ = new Vector3(target.position.x - spit.x, 0, target.position.z - spit.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 VelocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity));

        return VelocityXZ + velocityY;
    }

    //Animation call and sound call
    IEnumerator AttackAnimation() {
        animator.SetTrigger("Attack1");

        source.clip = attackAudio;
        source.Play();

        yield return new WaitForSeconds(0.5f);
        Spitting();
    }

    //spawn the spit
    void Spitting() {
        if ((transform.position - target.transform.position).magnitude > SpittingDistance - 1) {
            Transform Spit = Instantiate(SpitClass, spitBegin.position, Quaternion.identity);
            Spit.GetComponent<Rigidbody>().velocity = CalculateSpitVelocity(target.GetChild(0), Spit.position);
        }
    }
}
