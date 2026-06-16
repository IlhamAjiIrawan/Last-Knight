using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class ForestDragon : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 20f; 
    public float attackRange = 3.5f; 
    public float moveSpeed = 4f;
    public float damage = 25f;
    public float attackCooldown = 2f;

    [Header("Attack Delay Settings")]
    public float attackDelay = 0.8f; 
    public float attackRecoveryTime = 0.4f;

    [Header("Skill 1 Settings (Poison Gas - DOT)")]
    public float skill1Cooldown = 7f;     
    public float skill1Radius = 4f;             // Radius area lingkaran gas
    public float skill1DamagePerTick = 8f;      // Damage yang diterima player tiap detak (tick)
    public float skill1TelegraphDuration = 1.0f;// Durasi lampu merah/indikator sebelum gas keluar
    public float gasDuration = 5.0f;            // Berapa lama prefab gas aktif di map
    public float gasTickInterval = 0.5f;        // Jeda waktu antar damage (misal: tiap 0.5 detik)
    public float gasSpawnDistance = 3.5f;       // Jarak kemunculan gas di depan naga (jika spawn point kosong)

    [Header("Skill 1 Prefabs & Points")]
    public GameObject skill1DangerZoneCirclePrefab; 
    public Transform skill1DangerZoneSpawnPoint;    // Titik pusat gas (Bisa dikosongkan jika pakai gasSpawnDistance)
    public GameObject gasVFXPrefab;                 // Prefab partikel gas hijau beracun

    [Header("Skill 2 Settings (Side Slash Semi-Circle)")]
    public float skill2Cooldown = 8f;
    public float skill2Range = 5f;          
    public float skill2Angle = 140f;        
    public float skill2Damage = 35f;        
    public float skill2TelegraphDuration = 1.0f; 
    public float skill2PostHitDuration = 0.8f;   

    [Header("Skill 2 Prefabs & Visuals")]
    public GameObject skill2DangerZoneConePrefab; 
    public GameObject sideSlashPrefab;     
    public Transform sideSlashSpawnPoint;   

    [Header("Counter Attack (Defend & Scream) Settings")]
    public GameObject shieldPrefab;          
    public Transform shieldSpawnPoint;       
    public GameObject dangerZoneCirclePrefab; 
    [Tooltip("Tarik prefab efek angin/daun/scream ke sini")]
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

        if (health != null)
        {
            health.onDeath += HandleBossDeath;
            nextHPThreshold = health.maxHealth * 0.7f; 
        }

        lastSkill1Time = Time.time - (skill1Cooldown / 2f); 
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); 
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

        // AI mengecek apakah player berada dalam jangkauan tebasan gas kedepan
        float actualCheckDistance = skill1DangerZoneSpawnPoint != null ? 
            Vector3.Distance(skill1DangerZoneSpawnPoint.position, player.position) : (gasSpawnDistance + skill1Radius);

        if (distanceToPlayer <= actualCheckDistance && Time.time >= lastSkill1Time + skill1Cooldown)
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
        Debug.LogWarning("INTERRUPT: Forest Dragon memotong semua aksi untuk mengaktifkan Counter Attack!");
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
            anim.ResetTrigger("skill2"); 
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

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; 
        }

        anim.SetBool("isMoving", false);

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(attackDelay);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 1f && !isDead)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }

        yield return new WaitForSeconds(attackRecoveryTime);
        lastAttackTime = Time.time;
        isPreparingAttack = false;
    }

    IEnumerator Skill1Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // Penentuan posisi gas di depan naga secara dinamis
        Vector3 gasSpawnPos;
        if (skill1DangerZoneSpawnPoint != null) {
            gasSpawnPos = skill1DangerZoneSpawnPoint.position;
        } else {
            gasSpawnPos = transform.position + transform.forward * gasSpawnDistance;
        }
        gasSpawnPos.y = player.position.y + 0.03f; // Nempel tanah mengikuti tinggi player

        // 1. Memunculkan Indikator Bahaya Lingkaran
        if (skill1DangerZoneCirclePrefab != null)
        {
            currentSkillDangerZone = Instantiate(skill1DangerZoneCirclePrefab, gasSpawnPos, Quaternion.identity);
            currentSkillDangerZone.transform.localScale = new Vector3(skill1Radius * 2f, 1f, skill1Radius * 2f);
        }

        anim.SetTrigger("skill1"); 
        yield return new WaitForSeconds(skill1TelegraphDuration); 

        // Indikator hilang saat gas mulai menyembur
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        // 2. Memunculkan Prefab Efek Gas Beracun
        GameObject activeGasVFX = null;
        if (gasVFXPrefab != null)
        {
            activeGasVFX = Instantiate(gasVFXPrefab, gasSpawnPos, Quaternion.identity);
        }

        // Naga sudah bisa bergerak kembali setelah menyembur, sementara gasnya tertinggal di tanah
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        isUsingSkill = false; 
        lastSkill1Time = Time.time;

        // 3. Sistem Perulangan Damage (DOT) Selama Gas Masih Ada
        float elapsed = 0f;
        while (elapsed < gasDuration && !isDead)
        {
            // Deteksi objek berbasis area lingkaran di posisi gas berada
            Collider[] hitColliders = Physics.OverlapSphere(gasSpawnPos, skill1Radius);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    Health playerHealth = col.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(skill1DamagePerTick);
                        Debug.Log("<color=green>🤢 Player berdiri di dalam gas! Terkena " + skill1DamagePerTick + " Damage.</color>");
                    }
                }
            }

            yield return new WaitForSeconds(gasTickInterval);
            elapsed += gasTickInterval;
        }

        // 4. Gas lenyap total dan berhenti memberikan damage
        if (activeGasVFX != null) Destroy(activeGasVFX);
    }

    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (skill2DangerZoneConePrefab != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 0.03f, transform.position.z);
            currentSkillDangerZone = Instantiate(skill2DangerZoneConePrefab, spawnPos, transform.rotation);
            currentSkillDangerZone.transform.localScale = new Vector3(skill2Range * 2f, 1f, skill2Range * 2f);
        }

        anim.SetTrigger("skill2");
        yield return new WaitForSeconds(skill2TelegraphDuration + skill2PostHitDuration);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill2Time = Time.time;
        isUsingSkill = false;
    }

    public void TriggerSideSlashVFX()
    {
        if (isDead) return;

        if (sideSlashPrefab != null)
        {
            Vector3 vfxPosition = transform.position + transform.forward * 1.2f;
            Quaternion vfxRotation = transform.rotation;

            if (sideSlashSpawnPoint != null)
            {
                vfxPosition = sideSlashSpawnPoint.position;
                vfxRotation = sideSlashSpawnPoint.rotation;
            }
            else
            {
                vfxPosition.y += 1.2f; 
            }

            GameObject sideSlashVFX = Instantiate(sideSlashPrefab, vfxPosition, vfxRotation);
            Destroy(sideSlashVFX, 1.5f);
        }
    }

    public void TriggerSideSlashDamage()
    {
        if (isDead) return;

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        Collider[] colliders = Physics.OverlapSphere(transform.position, skill2Range);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                Vector3 dirToPlayer = (col.transform.position - transform.position).normalized;
                dirToPlayer.y = 0; 

                Vector3 forwardNoY = transform.forward;
                forwardNoY.y = 0;

                float angleToPlayer = Vector3.Angle(forwardNoY, dirToPlayer);

                if (angleToPlayer <= skill2Angle / 2f)
                {
                    Health playerHealth = col.GetComponent<Health>();
                    if (playerHealth != null) playerHealth.TakeDamage(skill2Damage);
                }
            }
        }
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
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null) playerHealth.TakeDamage(roarDamagePerTick); 
            }

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        if (currentSkillVFX != null)
        {
            ParticleSystem ps = currentSkillVFX.GetComponentInChildren<ParticleSystem>();
            if (ps != null) { ps.Stop(); Destroy(currentSkillVFX, 2f); }
            else { Destroy(currentSkillVFX); }
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
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (agent != null) agent.enabled = true; 
        if (health != null && health.healthSlider != null) Invoke("HideBossUI", 3f);
        
        this.enabled = false; 
    }

    void HideBossUI()
    {
        if (health != null && health.healthSlider != null) health.healthSlider.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, roarRange);

        Vector3 expectedGasPos = skill1DangerZoneSpawnPoint != null ? 
            skill1DangerZoneSpawnPoint.position : (transform.position + transform.forward * gasSpawnDistance);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(expectedGasPos, skill1Radius);

        // Gizmos Skill 2: Magenta 140°
        Gizmos.color = Color.magenta;
        Vector3 leftBoundaryS2 = Quaternion.Euler(0, -skill2Angle / 2f, 0) * transform.forward * skill2Range;
        Vector3 rightBoundaryS2 = Quaternion.Euler(0, skill2Angle / 2f, 0) * transform.forward * skill2Range;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundaryS2);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundaryS2);

        int segmentsS2 = 15;
        Vector3 previousPointS2 = transform.position + leftBoundaryS2;
        for (int i = 1; i <= segmentsS2; i++)
        {
            float currentAngle = -skill2Angle / 2f + (skill2Angle / segmentsS2) * i;
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, currentAngle, 0) * transform.forward * skill2Range;
            Gizmos.DrawLine(previousPointS2, nextPoint);
            previousPointS2 = nextPoint;
        }
    }
}