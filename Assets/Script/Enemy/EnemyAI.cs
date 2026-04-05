using UnityEngine;
using UnityEngine.AI; // Wajib untuk NavMesh

public class EnemyAI : MonoBehaviour
{
    public Transform player;      // Tarik objek Knight ke sini
    public float chaseRange = 10f; // Jarak musuh mulai mengejar
    public float attackRange = 2f; // Jarak musuh mulai menyerang
    public float moveSpeed = 3.5f;
    public float damage = 10f;
    private float attackCooldown = 1.5f;
    private float lastAttackTime;

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
        agent.isStopped = false;
        agent.SetDestination(player.position);
        
        // Atur parameter animasi jalan musuh (sesuaikan nama parameternya)
        anim.SetBool("isMoving", true);
    }

    void AttackPlayer()
    {
        agent.isStopped = true;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("attack");
            
            // Memberikan damage ke player
            player.GetComponent<Health>().TakeDamage(damage);
            
            lastAttackTime = Time.time;
        }
    }

    void StopMoving()
    {
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
}