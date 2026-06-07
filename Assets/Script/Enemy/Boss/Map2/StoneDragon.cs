using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class StoneDragon : MonoBehaviour
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

    [Header("Skill 2 Settings (Claw Semi-Circle)")]
    public float skill2Cooldown = 6f;
    public float skill2Range = 5f;
    [Tooltip("180 derajat berarti setengah lingkaran pas di depan boss")]
    public float skill2Angle = 180f; 
    public float skill2Damage = 35f;
    public float skill2TelegraphDuration = 0.6f; // Cakaran biasanya lebih instan/cepat
    public float skill2PostHitDuration = 0.8f;

    [Header("Danger Zone Visuals")]
    public GameObject dangerZoneBoxPrefab;   
    public GameObject dangerZoneConePrefab;  // Gunakan prefab cone/fan untuk area melingkar

    [Header("Skill 2 VFX Prefabs")]
    public GameObject clawVFXPrefab;         // Prefab efek visual sabetan cakar
    public Transform clawSpawnPoint;

    [Header("Counter Attack (Defend & Scream) Settings")]
    public GameObject shieldPrefab;          
    public Transform shieldSpawnPoint;       
    public GameObject dangerZoneCirclePrefab; 

    [Tooltip("Tarik prefab efek angin/scream ke sini")]
    public GameObject windPrefab;             

    public float roarRange = 15f;            
    public float roarDamagePerTick = 15f;    
    public float defendDuration = 2f;        
    public float screamDuration = 5f;        

    [Header("MECHANICS: Counter Attack Threshold")]
    private float nextHPThreshold;          
    private bool isCounterAttacking = false; 

    private NavMeshAgent agent;
    private Animator anim;
    private Health health;
    private bool isDead = false;
    private bool isPreparingAttack = false;
    private bool isUsingSkill = false;    
    private float lastAttackTime;
    private float lastSkill1Time;         
    private float lastSkill2Time;         

    private GameObject currentSkillDangerZone; 
    private GameObject currentSkillVFX;        

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

        health.onDeath += HandleBossDeath;
        
        lastSkill1Time = Time.time - (skill1Cooldown / 2f); 
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); // Mengatur jeda awal skill 2

        nextHPThreshold = health.maxHealth * 0.7f;
    }

    void Update()
    {
        if (isDead) return;

        if (!isCounterAttacking && health != null && health.currentHealth <= nextHPThreshold)
        {
            InterruptCurrentActions(); 
            StartCoroutine(CounterAttackRoutine());
            return;
        }

        if (isPreparingAttack || isUsingSkill || isCounterAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Prioritas: Skill 1 (Jarak Jauh) -> Skill 2 (Jarak Menengah/Setengah Lingkaran) -> Basic Attack
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

    void InterruptCurrentActions()
    {
        Debug.LogWarning("INTERRUPT: StoneDragon memotong semua aksi untuk mengaktifkan Counter Attack!");

        StopAllCoroutines();

        if (agent != null && !agent.enabled)
        {
            Vector3 groundPos = transform.position;
            groundPos.y = player.position.y; 
            transform.position = groundPos;
            agent.enabled = true;
        }

        isPreparingAttack = false;
        isUsingSkill = false;

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);
        if (currentSkillVFX != null) Destroy(currentSkillVFX);

        if (anim != null)
        {
            anim.ResetTrigger("attack");
            anim.ResetTrigger("skill1");
            anim.ResetTrigger("skill2"); // Reset trigger skill 2 jika terpotong
            anim.ResetTrigger("defend");
            anim.ResetTrigger("scream");
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

        if (dangerZoneBoxPrefab != null)
        {
            Vector3 spawnPos = new Vector3(boxCenter.x, transform.position.y + 0.02f, boxCenter.z);
            currentSkillDangerZone = Instantiate(dangerZoneBoxPrefab, spawnPos, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            currentSkillDangerZone.transform.localScale = new Vector3(skill1Width, jumpDistance, 1f);
        }

        anim.SetTrigger("skill1");
        yield return new WaitForSeconds(telegraphDuration);

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

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

    // --- BARU: LOGIKA SKILL 2 (CAKARAN SETENGAH LINGKARAN) ---
    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // Munculkan indikator area (Cone/Fan)
        if (dangerZoneConePrefab != null)
        {
            Vector3 spawnPos = clawSpawnPoint != null ? clawSpawnPoint.position : transform.position;
            spawnPos.y = transform.position.y + 0.05f;

            currentSkillDangerZone = Instantiate(dangerZoneConePrefab, spawnPos, transform.rotation);
            currentSkillDangerZone.transform.SetParent(transform);
            currentSkillDangerZone.transform.localScale = new Vector3(skill2Range, 1f, skill2Range);
        }

        // Memicu animasi mencakar
        anim.SetTrigger("skill2");
        
        // Tunggu sampai durasi indikator merah selesai sebelum menghapusnya
        yield return new WaitForSeconds(skill2TelegraphDuration);

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        // --- KODE SPAWN VFX DI SINI SUDAH DIHAPUS ---
        // Sekarang pemunculan VFX diserahkan penuh ke Animation Event (Fungsi TriggerClawVFX di bawah)

        // PERHITUNGAN DAMAGE AREA SETENGAH LINGKARAN
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, skill2Range);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player") && !isDead)
            {
                Vector3 dirToPlayer = (col.transform.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

                if (angleToPlayer <= (skill2Angle / 2f))
                {
                    col.GetComponent<Health>().TakeDamage(skill2Damage);
                }
                break; 
            }
        }

        yield return new WaitForSeconds(skill2PostHitDuration);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill2Time = Time.time;
        isUsingSkill = false;
    }

    // --- FUNGSI BARU: Dipanggil khusus melalui Animation Event ---
    public void TriggerClawVFX()
    {
        if (isDead || clawVFXPrefab == null) return;

        Vector3 spawnPosition = transform.position + transform.forward * 2f; // Lahir 2 meter di depan boss
        spawnPosition.y = transform.position.y + 1f; // Naikkan 1 meter dari tanah agar tidak tenggelam

        Transform spawnPoint = clawSpawnPoint != null ? clawSpawnPoint : transform;

        GameObject slashVFX = Instantiate(clawVFXPrefab, spawnPoint.position, spawnPoint.rotation);
        Destroy(slashVFX, 1.5f);
    }

    IEnumerator CounterAttackRoutine()
    {
        isCounterAttacking = true;
        isUsingSkill = true; 
        StopMoving();

        SetInvulnerable(true);
        nextHPThreshold -= health.maxHealth * 0.3f;

        anim.SetTrigger("defend"); 
        
        GameObject activeShield = null;
        if (shieldPrefab != null)
        {
            Transform spawnPoint = shieldSpawnPoint != null ? shieldSpawnPoint : transform;
            activeShield = Instantiate(shieldPrefab, spawnPoint.position, shieldSpawnPoint.rotation);
            
            if (spawnPoint.gameObject.scene.name == null)
                activeShield.transform.SetParent(this.transform);
            else
                activeShield.transform.SetParent(spawnPoint);

            activeShield.transform.localPosition = Vector3.zero;
            activeShield.transform.localRotation = Quaternion.identity;
            activeShield.transform.localScale = shieldPrefab.transform.localScale;
        }

        if (dangerZoneCirclePrefab != null)
        {
            Vector3 dangerPos = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
            currentSkillDangerZone = Instantiate(dangerZoneCirclePrefab, dangerPos, Quaternion.identity);
            currentSkillDangerZone.transform.SetParent(this.transform);
            currentSkillDangerZone.transform.localScale = new Vector3(roarRange * 2f, 0.1f, roarRange * 2f);
        }

        yield return new WaitForSeconds(defendDuration);

        if (activeShield != null) Destroy(activeShield);
        SetInvulnerable(false); 

        anim.SetTrigger("scream");

        if (windPrefab != null)
        {
            Transform spawnPoint = shieldSpawnPoint != null ? shieldSpawnPoint : transform;
            currentSkillVFX = Instantiate(windPrefab, spawnPoint.position, spawnPoint.rotation);
            currentSkillVFX.transform.SetParent(spawnPoint);
        }

        float elapsed = 0f;
        while (elapsed < screamDuration) 
        {
            if (isDead) break;

            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= roarRange)
            {
                player.GetComponent<Health>().TakeDamage(roarDamagePerTick); 
            }

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        if (currentSkillVFX != null)
        {
            ParticleSystem ps = currentSkillVFX.GetComponentInChildren<ParticleSystem>();
            if (ps != null) 
            { 
                ps.Stop(); 
                Destroy(currentSkillVFX, 2f); 
            }
            else 
            { 
                Destroy(currentSkillVFX); 
            }
        }

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        isUsingSkill = false;
        isCounterAttacking = false;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
    }

    void SetInvulnerable(bool invulnerable)
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = !invulnerable;
    }

    void HandleBossDeath()
    {
        isDead = true;
        StopAllCoroutines();
        SetInvulnerable(false); 
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);
        if (currentSkillVFX != null) Destroy(currentSkillVFX);
        if (agent != null) agent.enabled = true; 
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

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, roarRange);

        // GIZMOS SKILL 1 BOX
        Matrix4x4 skill1Matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Vector3 localCenter = new Vector3(0f, 0f, jumpDistance / 2f);
        Vector3 localSize = new Vector3(skill1Width, 0.1f, jumpDistance);
        Gizmos.DrawCube(localCenter, localSize);
        Gizmos.DrawWireCube(localCenter, localSize);
        Gizmos.matrix = skill1Matrix;

        // --- BARU: GIZMOS SKILL 2 (SETENGAH LINGKARAN / ARC) ---
        Gizmos.color = Color.orange;
        Vector3 leftBoundary = Quaternion.Euler(0, -skill2Angle / 2f, 0) * transform.forward * skill2Range;
        Vector3 rightBoundary = Quaternion.Euler(0, skill2Angle / 2f, 0) * transform.forward * skill2Range;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Menggambar garis lengkung luar setengah lingkaran
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