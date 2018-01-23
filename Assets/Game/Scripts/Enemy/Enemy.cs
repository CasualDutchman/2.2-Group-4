using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour {

    //components
    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource source;

    //health info
    public float health = 100;
    public float maxHealth = 100;

    //target info
    public Transform target;
    protected FirstPersonPlayerController PlayerScript;

    protected bool CanFollowPlayer = false;

    public float DelayBetweenAttacks = 1.0f;
    protected float TimeSinceLastAttack = 1.1f;

    //sound info
    public AudioClip[] audioIdle;
    public AudioClip attackAudio;

    //for animations
    protected bool isAttacking = false;

    //fire info
    public GameObject bodyFire;
    GameObject currentFire;
    float fireTimer = 0;

    //check if already slown down (fixed that speed went to 0)
    bool isSlowedDown = false;

    //setup
    protected virtual void Start () {
        health = maxHealth * DemoScript.instance.healthMultiplier;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = DemoScript.instance.speed;

        source = GetComponent<AudioSource>();

        StartCoroutine(Pathing());
        StartCoroutine(IdleSound());

        OnStart();
    }

    //Can be called in the child classes if stuff needs to be set up
    protected virtual void OnStart() { }

    //Set on fire
    public void SetFire() {
        fireTimer = 5;
        if (currentFire == null) {
            currentFire = Instantiate(bodyFire, transform);
        }
    }

    //Change speed
    public void SetSpeed(float f) {
        agent.speed = f;
    }

    //Lower the speed to a certain percent of the current speed
    public void LowerSpeed(float percent) {
        if(!isSlowedDown){
            agent.speed = agent.speed * percent;
            isSlowedDown = true;
        }
    }

    void Update() {
        UpdateLastAttackTime();
        UpdateAnimations();

        if (fireTimer > 0) {//when on fire, hurt the enemy
            fireTimer -= Time.deltaTime;
            Hurt(0.5f * Time.deltaTime);
        }
    }

    //Update animations in respective classes
    protected virtual void UpdateAnimations() { }

    //Idle Sounds
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

    //Pathing was in update and fixedupdate, but works just as fine here and takes less processing power
    IEnumerator Pathing() {
        while (true) {
            UpdateFindingPath();
            yield return new WaitForSeconds(0.3f);
        }
    }

    //used to find a new path
    protected virtual void UpdateFindingPath() {
        if (!isAttacking) {
            if (target != null) {
                if ((transform.position - target.transform.position).magnitude < 1.5f) {
                    agent.ResetPath();
                    Attack();
                } else {
                    agent.SetDestination(target.position);
                }
            }
        }
    }

    //update attack time
    protected void UpdateLastAttackTime() {
        TimeSinceLastAttack += Time.deltaTime;
        if (TimeSinceLastAttack >= 10.0f) TimeSinceLastAttack = DelayBetweenAttacks + 1;
    }

    //what happens on death
    public virtual void OnDeath() {
        GetComponent<RagdollManager>().EnableRagdoll();

        if (currentFire) {
            Destroy(currentFire);
        }

        Destroy(GetComponent<NavMeshAgent>());
        Destroy(this);
    }

    //When the enemy is hurt
    public void Hurt(float amount) {
        health -= amount * DemoScript.instance.damageMultiplier;
        if (health <= 0) {
            OnDeath();
        }
    }

    protected abstract void Attack();
}
