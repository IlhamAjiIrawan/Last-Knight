using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RangedEnemyAI : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 15f;
    public float attackRange = 10f;
    public float moveSpeed = 3.5f;
    private float attackCooldown = 2f;

    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;     // Prefab peluru/panah
    public Transform firePoint;             // Titik tempat peluru muncul (misal ujung tongkat/tangan)
    public float attackDelay = 0.5f;
    public float attackRecoveryTime = 0.4f;
    private bool isPreparingAttack = false;

    [Header("Flee / Kiting Settings")]
    [Tooltip("Jika player lebih dekat dari jarak ini, musuh akan otomatis menjauh")]
    public float fleeRange = 4f; 
    [Tooltip("Seberapa jauh musuh melangkah mundur saat menjauh")]
    public float fleeDistance = 6f; 

    [Header("Optimization Settings")]
    public float patrolDisableRange = 30f; 
    private bool isSleeping = false; 

    private float lastAttackTime;
    private bool isDead = false;
    private bool isStunned = false; 
    public float stunDuration = 0.5f; 

    private NavMeshAgent agent;
    private Animator anim;
    private float distanceToPlayer;

    [Header("Patrol Settings")]
    public float patrolRadius = 8f;     
    public float patrolWaitTime = 1f;    
    private float patrolTimer;
    private Vector3 patrolTarget;
    private bool hasPatrolTarget = false;
    private bool isKnockedBack = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.speed = moveSpeed;

        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Fitur Optimization Sleep
        if (distanceToPlayer > patrolDisableRange)
        {
            if (!isSleeping) EnterSleepMode();
            return; 
        }
        else
        {
            if (isSleeping) ExitSleepMode();
        }

        if (isStunned || isPreparingAttack || isKnockedBack) return;

        // LOGIKA KITING & MENYERANG
        if (distanceToPlayer <= fleeRange)
        {
            // Mekanik Baru: Player terlalu dekat, waktunya kabur/menjauh
            hasPatrolTarget = false;
            FleeFromPlayer();
        }
        else if (distanceToPlayer <= attackRange)
        {
            // Berada di zona aman menembak
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                StopMoving();
                // Tetap menghadap player meski sedang cooldown biar tidak kaku
                LookAtPlayer();
            }
        }
        else if (distanceToPlayer <= chaseRange)
        {
            hasPatrolTarget = false;
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void FleeFromPlayer()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        // Hitung arah berlawanan dari posisi player
        Vector3 directionToPlayer = transform.position - player.position;
        Vector3 fleeDestination = transform.position + directionToPlayer.normalized * fleeDistance;

        NavMeshHit hit;
        // Cari posisi valid di NavMesh agar musuh tidak menabrak dinding map saat mundur
        if (NavMesh.SamplePosition(fleeDestination, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            anim.SetBool("isMoving", true);
        }
    }

    void LookAtPlayer()
    {
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);
    }

    IEnumerator AttackRoutine()
    {
        isPreparingAttack = true;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        anim.SetBool("isMoving", false);

        LookAtPlayer();
        anim.SetTrigger("attack"); 

        yield return new WaitForSeconds(attackDelay);

        if (!isDead && !isStunned && projectilePrefab != null && firePoint != null)
        {
            Vector3 targetCenter = new Vector3(player.position.x, player.position.y + 1f, player.position.z);
            Vector3 launchDirection = (targetCenter - firePoint.position).normalized;

            Quaternion baseRotation = Quaternion.LookRotation(launchDirection);

            Quaternion finalRotation = baseRotation * Quaternion.Euler(0, 90, 0);

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, finalRotation);

            EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                projScript.SetupTarget(player);
            }
        }

        yield return new WaitForSeconds(attackRecoveryTime);
        lastAttackTime = Time.time;
        isPreparingAttack = false;
    }

    void EnterSleepMode()
    {
        isSleeping = true;
        hasPatrolTarget = false;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); }
        anim.SetBool("isMoving", false);
    }

    void ExitSleepMode()
    {
        isSleeping = false;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
    }

    void Patrol()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        if (!hasPatrolTarget)
        {
            patrolTarget = GetRandomPoint(transform.position, patrolRadius);
            agent.SetDestination(patrolTarget);
            agent.isStopped = false;
            anim.SetBool("isMoving", true);
            hasPatrolTarget = true;
            patrolTimer = 0f;
            return; 
        }
        if (agent.pathPending) return;
        if (agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            if (anim.GetBool("isMoving")) { anim.SetBool("isMoving", false); agent.isStopped = true; }
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime) hasPatrolTarget = false;
        }
    }

    Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius + center;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, radius, NavMesh.AllAreas)) return navHit.position;
        return center;
    }

    void ChasePlayer()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.SetDestination(player.position);
        anim.SetBool("isMoving", true);
    }

    void StopMoving()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        anim.SetBool("isMoving", false);
    }

    public void TakeHit()
    {
        if (isDead) return;
        isPreparingAttack = false; 
        hasPatrolTarget = false;
        StopCoroutine("StunRoutine");
        StopCoroutine("AttackRoutine"); 
        StartCoroutine("StunRoutine");
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;
        anim.SetBool("isMoving", false);
        anim.SetTrigger("getHit");
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        if (!isDead && agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
    }

    public void TakeKnockback(Vector3 direction, float force)
    {
        if (isDead) return;
        isPreparingAttack = false;
        hasPatrolTarget = false;
        StopAllCoroutines(); 
        StartCoroutine(KnockbackRoutine(direction, force));
    }

    IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        isKnockedBack = true;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;
        anim.SetTrigger("getHit"); 
        float duration = 0.2f, timer = 0f;
        while (timer < duration)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.Move(direction * force * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        isKnockedBack = false;
        StartCoroutine("StunRoutine");
    }

    public void OnDeath()
    {
        isDead = true;
        isPreparingAttack = false;
        StopAllCoroutines(); 
        if (agent != null) agent.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolDisableRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fleeRange);
    }
}