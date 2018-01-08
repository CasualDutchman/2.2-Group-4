using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAI : Enemy {

    public float damageDone = 10;

    bool didAttack = false;

    protected override void UpdateAnimations() {
        animator.SetFloat("Blend", agent.velocity.normalized.magnitude);

        if (isAttacking) {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime * animator.GetCurrentAnimatorStateInfo(0).speed >= 1 && !animator.IsInTransition(0)) {
                //isAttacking = false;
            }else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack_Melee")) {
                //isAttacking = false;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95) {
                    print(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                    isAttacking = false;
                }
            }

            if (didAttack)
                isAttacking = false;
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
