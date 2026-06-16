using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public float damage = 10f;
    private float attackCooldown = 1.5f;

    [Header("Optimization Settings")]
    [Tooltip("Jika jarak player melebihi angka ini, fitur patroli dimatikan agar game ringan")]
    public float patrolDisableRange = 25f; 
    private bool isSleeping = false; 

    [Header("New Attack Delay Settings")]
    public float attackDelay = 0.5f;
    public float attackRecoveryTime = 0.4f;
    private bool isPreparingAttack = false; 

    private float lastAttackTime;
    private bool isDead = false;
    private bool isStunned = false; // Variabel baru untuk status diam
    public float stunDuration = 0.5f; // Durasi diam saat dipukul

    private NavMeshAgent agent;
    private Animator anim;
    private float distanceToPlayer;

    [Header("Patrol Settings")]
    public float patrolRadius = 8f;     // Seberapa jauh musuh bisa memilih titik tujuan baru
    public float patrolWaitTime = 1f;    // Waktu tunggu di titik tujuan sebelum mencari titik baru
    private float patrolTimer;
    private Vector3 patrolTarget;
    private bool hasPatrolTarget = false;
    private bool isKnockedBack = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.speed = moveSpeed;

        // Jika lupa menarik player di Inspector, cari otomatis lewat Tag
        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        Health h = GetComponent<Health>();
        if (h != null)
        {
            h.onDeath += OnDeath;
        }
    }

    void Update()
    {
        // Jika mati, jangan lakukan apapun
        if (isDead) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > patrolDisableRange)
        {
            if (!isSleeping)
            {
                EnterSleepMode();
            }
            return; // Potong logic Update ke bawah agar CPU tidak memproses pathfinding & state lainnya
        }
        else
        {
            if (isSleeping)
            {
                ExitSleepMode();
            }
        }

        // Jika sedang tertegun (stun), bersiap serang, atau terpental, lewatkan logic di bawah
        if (isStunned || isPreparingAttack || isKnockedBack) return;

        if (distanceToPlayer <= attackRange)
        {
            // Cek cooldown sebelum memulai rangkaian serangan
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                StopMoving();
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

    void EnterSleepMode()
    {
        isSleeping = true;
        hasPatrolTarget = false; // Reset target patroli agar saat bangun mencari rute baru

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath(); // Hapus sisa kalkulasi jalur di memori NavMesh
        }

        anim.SetBool("isMoving", false);
    }

    void ExitSleepMode()
    {
        isSleeping = false;
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    void Patrol()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        // 1. Jika belum punya titik tujuan, cari titik acak baru
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

        // 2. Tambahan Cek: Pastikan NavMesh tidak sedang menghitung jalur (Path Pending)
        if (agent.pathPending) return;

        // 3. Cek apakah sudah mendekati titik tujuan
        if (agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            // Pastikan animasi bergerak mati saat sampai
            if (anim.GetBool("isMoving"))
            {
                anim.SetBool("isMoving", false);
                agent.isStopped = true;
            }

            // Jalankan timer tunggu di tempat
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                hasPatrolTarget = false; // Reset ke false agar frame berikutnya mencari titik baru!
            }
        }
    }

    Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, radius, NavMesh.AllAreas))
        {
            return navHit.position;
        }

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

    // Untuk visualisasi jarak di Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // 🔥 BARU: Menggambar lingkaran batas tidur musuh (Warna Biru)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolDisableRange);
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
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        anim.SetBool("isMoving", false);
        anim.SetTrigger("getHit");

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        
        if (!isDead && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
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

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true; 
        }

        anim.SetTrigger("getHit"); 

        float duration = 0.2f; 
        float timer = 0f;

        while (timer < duration)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.Move(direction * force * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
        StartCoroutine("StunRoutine");
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

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(attackDelay);

        float currentDistance = Vector3.Distance(transform.position, player.position);
        if (currentDistance <= attackRange + 0.5f && !isDead && !isStunned)
        {
            player.GetComponent<Health>().TakeDamage(damage);
        }
        
        yield return new WaitForSeconds(attackRecoveryTime);
        lastAttackTime = Time.time;
        isPreparingAttack = false;
    }

    public void OnDeath()
    {
        isDead = true;
        isPreparingAttack = false;
        StopAllCoroutines(); 
        if (agent != null) agent.enabled = false;
    }
}