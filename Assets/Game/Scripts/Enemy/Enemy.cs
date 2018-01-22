using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour {

    public float health = 100;
    public float maxHealth = 100;

    public Transform target;
    protected FirstPersonPlayerController PlayerScript;

    protected bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 1.0f;
    protected float TimeSinceLastAttack = 1.1f;

    protected NavMeshAgent agent;
    protected Animator animator;

    public AudioClip[] audioIdle;
    public AudioClip attackAudio;
    protected AudioSource source;

    //for animations
    protected bool isAttacking = false;

    protected virtual void Start () {
        health = maxHealth * DemoScript.instance.healthMultiplier;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = DemoScript.instance.speed;

        source = GetComponent<AudioSource>();

        StartCoroutine(Pathing());
        StartCoroutine(IdleSound());
    }

    public void SetSpeed(float f) {
        agent.speed = f;
    }

    void Update() {
        UpdateLastAttackTime();
        UpdateAnimations();
    }

    protected virtual void UpdateAnimations() { }

    IEnumerator IdleSound() {
        float rand = Random.Range(5.0f, 8.5f);
        while (true) {
            if (!isAttacking) {
                source.clip = audioIdle[Random.Range(0, audioIdle.Length)];
                source.Play();
            }
            yield return new WaitForSeconds(rand);
            rand = Random.Range(5.0f, 8.5f);
        }
    }

    void FixedUpdate () {
        //UpdateFindingPath();
    }

    IEnumerator Pathing() {
        while (true) {
            UpdateFindingPath();
            yield return new WaitForSeconds(0.3f);
        }
    }

    protected virtual void UpdateFindingPath() {
        if (!isAttacking) {
            if (!CanFollowPlayer) {
                DetectPlayerLineOfSight();
            } else if (target != null) {
                if ((transform.position - target.transform.position).magnitude < 1.5f) {
                    agent.ResetPath();
                    Attack();
                } else {
                    agent.SetDestination(target.position);
                }
            }
        }
    }

    protected void UpdateLastAttackTime() {
        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    protected void DetectPlayerLineOfSight() {
        RaycastHit OutHit;
        if (target != null) {
            //if (Physics.Linecast(transform.position, target.transform.position, out OutHit)) {
                //if (OutHit.collider.gameObject.tag == "Player") {
                    CanFollowPlayer = true;
                //}
            //}
        }
    }

    public virtual void OnDeath() {
        GetComponent<RagdollManager>().EnableRagdoll();

        Destroy(GetComponent<NavMeshAgent>());
        Destroy(this);
    }

    public void Hurt(float amount) {
        health -= amount * DemoScript.instance.damageMultiplier;
        if (health <= 0) {
            OnDeath();
        }
    }

    protected abstract void Attack();
}
