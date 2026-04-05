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
        if (isDead || isStunned) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
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
        
        // Hentikan coroutine lama jika ada, lalu mulai yang baru
        StopCoroutine("StunRoutine");
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

    public void OnDeath()
    {
        isDead = true;
        StopAllCoroutines(); // Hentikan stun jika mati
        if (agent != null) agent.enabled = false;
    }
}