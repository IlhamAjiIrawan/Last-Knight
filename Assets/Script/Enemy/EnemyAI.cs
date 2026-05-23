using UnityEngine;
using UnityEngine.AI; // Wajib untuk NavMesh
using System.Collections; // Wajib untuk IEnumerator

public class EnemyAI : MonoBehaviour
{
    public Transform player;      // Tarik objek Knight ke sini
    public float chaseRange = 10f; // Jarak musuh mulai mengejar
    public float attackRange = 2f; // Jarak musuh mulai menyerang
    public float moveSpeed = 3.5f;
    public float damage = 10f;
    private float attackCooldown = 1.5f;

    [Header("New Attack Delay Settings")]
    public float attackDelay = 0.5f; // Jeda sebelum serangan benar-benar kena (Ancang-ancang)
    private bool isPreparingAttack = false; // Status apakah sedang ancang-ancang

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
    }

    void Update()
    {
        // Jika mati atau sedang tertegun (stun), jangan lakukan apapun
        if (isDead || isStunned || isPreparingAttack || isKnockedBack) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

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
            return; // KELUAR DULU dari fungsi agar NavMesh punya waktu menghitung jalur di frame berikutnya
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
        // Cari posisi acak di dalam lingkaran khayal
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit navHit;
        // Cari titik terdekat yang valid di dalam NavMesh berdasarkan posisi acak tadi
        if (NavMesh.SamplePosition(randomDirection, out navHit, radius, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return center; // Kembalikan ke posisi awal jika gagal menemukan titik valid
    }

    void ChasePlayer()
    {
        // Cek apakah agent aktif dan sedang menempel di NavMesh
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
        anim.SetBool("isMoving", true);
    }

    void AttackPlayer()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.isStopped = true;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("attack");
            player.GetComponent<Health>().TakeDamage(damage);
            lastAttackTime = Time.time;
        }
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
    }

    // Fungsi baru untuk dipanggil saat kena pukul
    public void TakeHit()
    {
        if (isDead) return;
        isPreparingAttack = false; 
        hasPatrolTarget = false;
        
        StopCoroutine("StunRoutine");
        StopCoroutine("AttackRoutine"); // Batalkan serangan jika kena pukul
        StartCoroutine("StunRoutine");
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        
        // Hentikan navigasi agar tidak meluncur saat getHit
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        anim.SetBool("isMoving", false);
        anim.SetTrigger("getHit");

        // Tunggu selama durasi stun
        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        
        // Kembalikan navigasi jika masih hidup
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

        StopAllCoroutines(); // Hentikan patroli, stun lama, atau serangan yang sedang bersiap
        StartCoroutine(KnockbackRoutine(direction, force));
    }

    IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        isKnockedBack = true;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true; // Hentikan perintah gerak AI
        }

        anim.SetTrigger("getHit"); // Putar animasi terkejut/terluka

        float duration = 0.2f; // Durasi pentalan (singkat saja agar terasa responsif)
        float timer = 0f;

        while (timer < duration)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                // Dorong posisi musuh menggunakan agent.Move secara halus
                agent.Move(direction * force * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;

        // Setelah selesai terpental, berikan efek Stun/diam sejenak agar player bisa mengejar
        StartCoroutine("StunRoutine");
    }

    // Logic serangan baru dengan Coroutine untuk Delay
    IEnumerator AttackRoutine()
    {
        isPreparingAttack = true;
        agent.isStopped = true;
        anim.SetBool("isMoving", false);

        // Menghadap ke player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // 1. Trigger animasi Attack (Ancang-ancang dimulai)
        anim.SetTrigger("attack");

        // 2. Jeda waktu sebelum damage dikirim (Pemain punya waktu buat Dash menjauh!)
        yield return new WaitForSeconds(attackDelay);

        // 3. Cek lagi, apakah setelah delay pemain masih di dalam jangkauan?
        float currentDistance = Vector3.Distance(transform.position, player.position);
        if (currentDistance <= attackRange + 0.5f && !isDead && !isStunned)
        {
            player.GetComponent<Health>().TakeDamage(damage);
        }

        lastAttackTime = Time.time;
        isPreparingAttack = false;
    }

    public void OnDeath()
    {
        isDead = true;
        isPreparingAttack = false;
        StopAllCoroutines(); // Hentikan stun jika mati
        if (agent != null) agent.enabled = false;
    }
}