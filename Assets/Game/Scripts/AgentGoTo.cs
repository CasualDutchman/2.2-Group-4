using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentGoTo : MonoBehaviour {

    public Transform target;

    public Transform eyes;

    private bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 5.0f;
    private float TimeSinceLastAttack = 0;

    NavMeshAgent agent;

    public float health = 100f;

    bool attack;

	void Start () {
        agent = GetComponent<NavMeshAgent>();

        FindTarget();
    }

    void FindTarget() {
        GameObject[] list = GameObject.FindGameObjectsWithTag("Player");

        if (list.Length <= 0)
            return;

        float[] distances = new float[list.Length];
        int closestIndex = 0;
        for (int i = 0; i < list.Length; i++) {
            distances[i] = Vector3.Distance(transform.position, list[i].transform.position);

            if (i > 0 && distances[i] < distances[i - 1]) {
                closestIndex = i;
            }
        }

        target = list[closestIndex].transform.GetChild(0);
    }

    void OnParticleCollision(GameObject other) {
        health -= 2;

        if (health <= 0) {
            Destroy(gameObject);
        }
    }

    void FixedUpdate () {
        FindTarget();

        if (!CanFollowPlayer && target != null) {
            RaycastHit OutHit;
            if (!Physics.Linecast(eyes.position, target.position, out OutHit)) {
                CanFollowPlayer = true;
            }
        } else if(target != null){
            if (!attack) {
                agent.SetDestination(target.position);
            }

            if ((transform.position - target.transform.position).magnitude < 2f && !attack) {
                Attack();
            }
        }

        TimeSinceLastAttack = Mathf.Clamp(TimeSinceLastAttack - Time.deltaTime, 0, float.MaxValue);
        if (TimeSinceLastAttack <= 0) {
            attack = false;
        }
    }

    private void Attack() {
        TimeSinceLastAttack = DelayBetweenAttacks;
        agent.ResetPath();
        target.parent.GetComponent<Player>().Hurt();

        attack = true;

        print("Attack");
    }
}
