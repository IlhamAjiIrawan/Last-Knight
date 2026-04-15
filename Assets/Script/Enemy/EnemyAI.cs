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
        if (isDead || isStunned || isPreparingAttack) return;

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
                StopMoving(); // Diam di tempat sambil menunggu cooldown
            }
        }
        else if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
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
        
        // Jika sedang ancang-ancang menyerang lalu dipukul, batalkan serangannya!
        isPreparingAttack = false; 
        
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