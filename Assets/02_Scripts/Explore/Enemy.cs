using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyState {
        Patrol,
        Chase,
        Attack,
        Damaged,
        Die
    }

    public EnemyState currentState = EnemyState.Patrol;

    private float currentHp;

    [Header("References")]
    public EnemyData data;
    private PlayerController player;
    private Transform target;
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime = 0f;
    private bool isDead = false;

    private Vector3 spawnPosition;
    private float wanderTimer = 0f;

    public GameObject attackHitbox;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    public void Setup(PlayerController playerRef)
    {
        this.player = playerRef;

        if (this.player != null)
        {
            this.player.OnPlayerDied += ClearTarget;
            Initialize();
        }
    }

    void OnDestroy()
    {
        if (player != null) player.OnPlayerDied -= ClearTarget;

    }

    public void Initialize()
    {
        if (data == null) return;

        currentHp = data.maxHp;
        agent.speed = data.moveSpeed;

        target = player.transform;

        spawnPosition = transform.position;
        currentState = EnemyState.Patrol;

        MoveToRandomLocation();
    }

    void Update()
    {
        if (isDead) return;

        animator.SetFloat("Speed", agent.velocity.magnitude);

        switch (currentState)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Chase: Chase(); break;
            case EnemyState.Attack: Attack(); break;
        }
    }

    void Patrol()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= data.detectRange)
            {
                currentState = EnemyState.Chase;
                agent.speed = data.runSpeed;
                wanderTimer = 0f;
                return;
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= data.patrolWaitTime)
            {
                MoveToRandomLocation();
                wanderTimer = 0f;
            }
        }
    }

    void MoveToRandomLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * data.patrolRadius;
        randomDirection += spawnPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, data.patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = data.moveSpeed;
        }
    }

    void Chase()
    {
        if (target == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, target.position);

        if (distToPlayer <= data.attackRange)
        {
            currentState = EnemyState.Attack;
            agent.ResetPath();
            return;
        }

        if (distToPlayer > data.detectRange)
        {
            currentState = EnemyState.Patrol;
            agent.speed = data.moveSpeed;
            MoveToRandomLocation(); 
            return;
        }

        agent.SetDestination(target.position);
    }

    void Attack()
    {
        if (target == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        if (Time.time - lastAttackTime > data.attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");
        }

        float distToPlayer = Vector3.Distance(transform.position, target.position);
        if (distToPlayer > data.attackRange)
        {
            currentState = EnemyState.Chase;
            agent.speed = data.runSpeed;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"HP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(OnHitRoutine());
        }
    }

    IEnumerator OnHitRoutine()
    {
        EnemyState previousState = currentState;
        currentState = EnemyState.Damaged;
        agent.isStopped = true;
        // animator.SetTrigger("Hit"); 

        yield return new WaitForSeconds(0.5f);

        if (!isDead)
        {
            agent.isStopped = false;
            currentState = EnemyState.Chase;
            agent.speed = data.runSpeed;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        currentState = EnemyState.Die;

        agent.isStopped = true;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        // animator.SetTrigger("Die"); 

        if (data.deathEffect != null)
        {
            Instantiate(data.deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 2f);
    }

    void ClearTarget()
    {
        target = null;

    }
    public void EnableAttackHitBox() => attackHitbox.SetActive(true);
    public void DisableAttackHitBox() => attackHitbox.SetActive(false);
}