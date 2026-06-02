using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class RedDragon : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 20f; 
    public float attackRange = 3.5f; 
    public float moveSpeed = 4f;
    public float damage = 25f;
    public float attackCooldown = 2f;

    [Header("Attack Delay Settings")]
    public float attackDelay = 0.8f; 

    [Header("Skill 1 Settings (Jump Attack Box)")]
    public float skill1Cooldown = 8f;     
    public float skill1Range = 10f;       
    public float skill1Damage = 40f;      
    public float jumpDistance = 6f;       
    public float skill1Width = 4f;        
    public float telegraphDuration = 1.2f; 
    public float postHitDuration = 1.5f;   

    [Header("Skill 2 Settings (Fire Breath Cone)")]
    public float skill2Cooldown = 12f;
    public float skill2Range = 7f;         
    public float skill2Angle = 90f;        
    public float skill2DamagePerTick = 10f; 
    public float skill2TelegraphDuration = 1.0f; // Jeda Danger Zone menyala
    public float skill2Duration = 2.5f;    // Berapa lama api menyembur & damage aktif

    [Header("Danger Zone Visuals")]
    public GameObject dangerZoneBoxPrefab;   
    public GameObject dangerZoneConePrefab;  

    [Header("Skill 2 VFX Prefabs")]
    [Tooltip("Masukkan Prefab efek Partikel Api / Semburan Api di sini")]
    public GameObject fireBreathPrefab;      
    [Tooltip("Buat objek kosong di moncong naga, lalu masukkan ke sini agar api keluar dari mulut")]
    public Transform fireSpawnPoint;         

    private NavMeshAgent agent;
    private Animator anim;
    private Health health;
    private bool isDead = false;
    private bool isPreparingAttack = false;
    private bool isUsingSkill = false;    
    private float lastAttackTime;
    private float lastSkill1Time;         
    private float lastSkill2Time;         

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();

        if (agent != null) agent.speed = moveSpeed;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        GameObject bossUIObj = GameObject.Find("BossHPBar"); 
        if (bossUIObj != null)
        {
            Slider bossSlider = bossUIObj.GetComponent<Slider>();
            bossSlider.gameObject.SetActive(true); 
            health.healthSlider = bossSlider; 
            bossSlider.maxValue = health.maxHealth;
            bossSlider.value = health.maxHealth;
        }

        health.onDeath += HandleBossDeath;
        
        lastSkill1Time = Time.time - (skill1Cooldown / 2f); 
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); 
    }

    void Update()
    {
        if (isDead || isPreparingAttack || isUsingSkill) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= skill1Range && Time.time >= lastSkill1Time + skill1Cooldown)
        {
            StartCoroutine(Skill1Routine());
        }
        else if (distanceToPlayer <= skill2Range && Time.time >= lastSkill2Time + skill2Cooldown)
        {
            StartCoroutine(Skill2Routine());
        }
        else if (distanceToPlayer <= attackRange)
        {
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
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
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

    IEnumerator AttackRoutine()
    {
        isPreparingAttack = true;
        agent.isStopped = true;
        anim.SetBool("isMoving", false);

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(attackDelay);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 1f && !isDead)
        {
            player.GetComponent<Health>().TakeDamage(damage);
        }

        lastAttackTime = Time.time;
        isPreparingAttack = false;
    }

    IEnumerator Skill1Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        Vector3 boxCenter = transform.position + transform.forward * (jumpDistance / 2f);

        GameObject activeDangerZone = null;
        if (dangerZoneBoxPrefab != null)
        {
            Vector3 spawnPos = new Vector3(boxCenter.x, transform.position.y + 0.02f, boxCenter.z);
            activeDangerZone = Instantiate(dangerZoneBoxPrefab, spawnPos, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            activeDangerZone.transform.localScale = new Vector3(skill1Width, jumpDistance, 1f);
        }

        anim.SetTrigger("skill1");
        yield return new WaitForSeconds(telegraphDuration);

        if (activeDangerZone != null) Destroy(activeDangerZone);

        Vector3 boxHalfExtents = new Vector3(skill1Width / 2f, 2f, jumpDistance / 2f);
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, boxHalfExtents, transform.rotation);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player") && !isDead)
            {
                col.GetComponent<Health>().TakeDamage(skill1Damage);
                break; 
            }
        }

        yield return new WaitForSeconds(postHitDuration); 

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill1Time = Time.time;
        isUsingSkill = false;
    }

    // ====================================================================
    // SKILL 2: SEMBURAN API
    // ====================================================================
    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        // 1. Kunci pandangan naga ke Player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // 2. Munculkan Danger Zone berbentuk Kipas di lantai
        GameObject activeDangerZone = null; 
        if (dangerZoneConePrefab != null)
        {
            Vector3 spawnPos = fireSpawnPoint != null ? fireSpawnPoint.position : transform.position;
            spawnPos.y = transform.position.y + 0.05f;

            // Spawn di posisi target mengikuti arah hadap naga saat ini
            activeDangerZone = Instantiate(dangerZoneConePrefab, spawnPos, transform.rotation);
            activeDangerZone.transform.SetParent(transform);

            // --- PERBAIKAN: MENGIKUTI SETTINGAN ASLI PREFAB SECARA STATIS ---
            // Memaksa nilai rotasi lokal dan skala kembali 100% ke setelan bawaan file Prefab kamu
            activeDangerZone.transform.localRotation = dangerZoneConePrefab.transform.localRotation;
            activeDangerZone.transform.localScale = dangerZoneConePrefab.transform.localScale;
        }

        // Picu animasi menyembur api
        anim.SetTrigger("skill2");

        // Tunggu hingga Fase Lampu Merah (Telegraph) selesai
        yield return new WaitForSeconds(skill2TelegraphDuration);

        // Hancurkan indikator merah di lantai tepat saat api keluar
        if (activeDangerZone != null) Destroy(activeDangerZone);


        // --- MUNCILLKAN PREFAB API ---
        GameObject activeFireVFX = null;
        if (fireBreathPrefab != null)
        {
            Transform spawnPoint = fireSpawnPoint != null ? fireSpawnPoint : transform;
            activeFireVFX = Instantiate(fireBreathPrefab, spawnPoint.position, spawnPoint.rotation);
            activeFireVFX.transform.SetParent(spawnPoint);
        }


        // 3. FASE MENYEMBUR (DAMAGE OVER TIME)
        float elapsed = 0f;
        while (elapsed < skill2Duration)
        {
            if (isDead) break;

            // Logika area hit damage asli (tetap menggunakan variabel inspector agar damage-nya akurat)
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= skill2Range && angleToPlayer <= (skill2Angle / 2f))
            {
                player.GetComponent<Health>().TakeDamage(skill2DamagePerTick);
                Debug.Log("Player terbakar semburan api naga!");
            }

            yield return new WaitForSeconds(0.5f); 
            elapsed += 0.5f;
        }

        // MATIKAN/HANCURKAN PREFAB API SETELAH DURASI SELESAI
        if (activeFireVFX != null)
        {
            ParticleSystem ps = activeFireVFX.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                Destroy(activeFireVFX, 2f); 
            }
            else
            {
                Destroy(activeFireVFX); 
            }
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill2Time = Time.time;
        isUsingSkill = false;
    }

    void HandleBossDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (health.healthSlider != null) Invoke("HideBossUI", 3f);
        this.enabled = false; 
    }

    void HideBossUI()
    {
        if (health.healthSlider != null) health.healthSlider.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, skill1Range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Vector3 localCenter = new Vector3(0f, 0f, jumpDistance / 2f);
        Vector3 localSize = new Vector3(skill1Width, 0.1f, jumpDistance);
        Gizmos.DrawCube(localCenter, localSize);
        Gizmos.DrawWireCube(localCenter, localSize);
        Gizmos.matrix = originalMatrix;

        Gizmos.color = Color.orange;
        Vector3 leftBoundary = Quaternion.Euler(0, -skill2Angle / 2f, 0) * transform.forward * skill2Range;
        Vector3 rightBoundary = Quaternion.Euler(0, skill2Angle / 2f, 0) * transform.forward * skill2Range;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        int segments = 10;
        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -(skill2Angle / 2f) + ((skill2Angle / segments) * i);
            Vector3 nextPoint = transform.position + (Quaternion.Euler(0, currentAngle, 0) * transform.forward * skill2Range);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}