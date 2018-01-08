using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpittingAI : Enemy {
    public float SpittingDistance = 5.0f;
    public float SpittingForce = 1000.0f;

    public Transform SpitClass;

    public Transform spitBegin;

    protected override void UpdateFindingPath() {
        if (!CanFollowPlayer) {
            DetectPlayerLineOfSight();
        }
        else {
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

    protected override void Attack() {
        if (TimeSinceLastAttack >= DelayBetweenAttacks) {
            TimeSinceLastAttack = 0.0f;
            Transform Spit = Instantiate(SpitClass, spitBegin.position, Quaternion.identity);
            Spit.GetComponent<Rigidbody>().velocity = CalculateSpitVelocity(target.GetChild(0), Spit);
        }
    }

    Vector3 CalculateSpitVelocity(Transform target, Transform spit) {
        float gravity = Physics.gravity.y;
        float maxheight = Mathf.Max(target.position.y, spit.position.y);
        float h = maxheight + 0.2f;

        float displacementY = target.position.y - spit.position.y;
        Vector3 displacementXZ = new Vector3(target.position.x - spit.position.x, 0, target.position.z - spit.position.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 VelocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity));

        return VelocityXZ + velocityY;
    }
}
