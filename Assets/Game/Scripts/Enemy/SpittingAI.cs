using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpittingAI : Enemy {
    public float SpittingDistance = 5.0f;

    public Transform SpitClass;
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
        animator.SetFloat("Blend", agent.velocity.normalized.magnitude);

        if (isAttacking) {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95) {
                    isAttacking = false;
                }
            }
        }
    }

    protected override void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;

            if (!isAttacking) {
                StartCoroutine(AttackAnimation());
                isAttacking = true;
            }
        }
    }

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

    IEnumerator AttackAnimation() {
        animator.SetTrigger("Attack1");

        source.clip = attackAudio;
        source.Play();

        yield return new WaitForSeconds(0.5f);
        Spitting();
        //yield return null;
    }

    void Spitting() {
        if ((transform.position - target.transform.position).magnitude > SpittingDistance - 1) {
            Transform Spit = Instantiate(SpitClass, spitBegin.position, Quaternion.identity);
            Spit.GetComponent<Rigidbody>().velocity = CalculateSpitVelocity(target.GetChild(0), Spit.position);
        }
    }
}
