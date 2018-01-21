using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAI : Enemy {

    public float damageDone = 10;

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

    IEnumerator AttackAnimation() {
        animator.SetTrigger("Attack1");
        
        yield return new WaitForSeconds(0.4f);
        HurtWhenInRange();
        //yield return null;
    }

    void HurtWhenInRange() {
        if ((transform.position - target.transform.position).magnitude < 2f) {
            target.GetComponent<Player>().Hurt(damageDone);
        }
    }
}
