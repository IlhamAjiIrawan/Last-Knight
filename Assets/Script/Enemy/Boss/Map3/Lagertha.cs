using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class WizardBossAI : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 25f; 
    public float attackRange = 12f; 
    public float moveSpeed = 4f;
    public float damage = 20f;
    public float attackCooldown = 2f;

    [Header("Basic Attack Settings")]
    public GameObject basicProjectilePrefab; 
    public Transform firePoint;              
    public float attackDelay = 0.6f; 

    [Header("Skill 1 Settings (Consecutive Straight Lasers - Box Area)")]
    public float skill1Cooldown = 8f;     
    public float skill1Range = 15f;           // Panjang kotak ke depan
    public float skill1Width = 5f;            // Lebar kotak kesamping
    public GameObject laserPrefab;           
    public Transform skill1SpawnPoint;       
    public int laserCount = 5;               
    public float delayBetweenLasers = 0.15f; 
    public float skill1TelegraphDuration = 1.0f;
    public GameObject skill1DangerZoneBoxPrefab; // Prefab Indikator berbentuk Kotak/Cube

    [Header("Skill 2 Settings (Instant Frost & Ice Patch)")]
    public float skill2Cooldown = 12f;
    public float skill2Range = 7f;        
    public float skill2Angle = 90f;       
    public float skill2InstantDamage = 35f;
    public float skill2TelegraphDuration = 1.0f;
    
    [Header("Ice/Magic Trap Settings")]
    public GameObject iceFloorPrefab;
    public Transform iceSpawnPoint;
    public float icePatchDuration = 5f;

    [Header("Danger Zone Visuals")]
    public GameObject dangerZoneCirclePrefab; 
    public GameObject dangerZoneConePrefab;   // Digunakan untuk Skill 2

    [Header("Counter Attack (Defend & Instant Freeze Blast) Settings")]
    public GameObject shieldPrefab;          
    public Transform shieldSpawnPoint;       
    public GameObject dangerZoneCounterCirclePrefab; 
    public GameObject instantBlastVFX;              
    public float counterBlastRange = 15f;            
    public float counterBlastDamage = 40f;           
    public float freezeDuration = 3f;                
    public float defendDuration = 2f;        

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
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); 

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
                LookAtPlayer();
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
        Debug.LogWarning("INTERRUPT: Wizard Boss memotong aksi untuk Counter Attack!");
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
            anim.ResetTrigger("counterBlast");
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

    void LookAtPlayer()
    {
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    IEnumerator AttackRoutine()
    {
        isPreparingAttack = true;
        agent.isStopped = true;
        anim.SetBool("isMoving", false);

        LookAtPlayer();
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(attackDelay);

        if (!isDead && basicProjectilePrefab != null && firePoint != null)
        {
            Vector3 targetDir = (new Vector3(player.position.x, player.position.y + 1f, player.position.z) - firePoint.position).normalized;
            GameObject proj = Instantiate(basicProjectilePrefab, firePoint.position, Quaternion.LookRotation(targetDir));
            
            EnemyProjectile eProj = proj.GetComponent<EnemyProjectile>();
            if (eProj != null) eProj.damage = this.damage;
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

        // 2. Munculkan indikator Danger Zone kotak memanjang sesuai rumus IceDragon
        if (skill1DangerZoneBoxPrefab != null)
        {
            // Hitung titik tengah area kotak di depan Boss (setengah dari skill1Range)
            Vector3 boxCenter = transform.position + transform.forward * (skill1Range / 2f);
            Vector3 spawnPos = new Vector3(boxCenter.x, transform.position.y + 0.02f, boxCenter.z);
            
            // Spawn dengan memutar objek 90 derajat di sumbu X (Sama seperti IceDragon)
            currentSkillDangerZone = Instantiate(skill1DangerZoneBoxPrefab, spawnPos, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            
            // Mengatur Skala: X = Lebar Kotak, Y = Jangkauan/Panjang ke Depan, Z = 1f
            currentSkillDangerZone.transform.localScale = new Vector3(skill1Width, skill1Range, 1f);
        }

        anim.SetTrigger("skill1");
        yield return new WaitForSeconds(skill1TelegraphDuration);

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        if (!isDead && laserPrefab != null)
        {
            Transform spawnPoint = (skill1SpawnPoint != null) ? skill1SpawnPoint : firePoint;

            if (spawnPoint != null)
            {
                Vector3 spawnDir = transform.forward; // Arah tembakan lurus mutlak ke depan
                float horizontalSpreadFactor = skill1Width * 0.7f; // Ambil area sebaran aman di dalam kotak
                float startOffset = -horizontalSpreadFactor / 2f;

                for (int i = 0; i < laserCount; i++)
                {
                    if (isDead) break;
                    float currentOffset = (laserCount > 1) ? startOffset + (i * (horizontalSpreadFactor / (laserCount - 1))) : 0f;
                    Vector3 dynamicSpawnPos = spawnPoint.position + (transform.right * currentOffset);
                    Instantiate(laserPrefab, dynamicSpawnPos, Quaternion.LookRotation(spawnDir));
                    yield return new WaitForSeconds(delayBetweenLasers);
                }
            }
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill1Time = Time.time;
        isUsingSkill = false;
    }

    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        Vector3 targetFloorPosition = player.position;

        if (dangerZoneConePrefab != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y = transform.position.y + 0.05f;

            currentSkillDangerZone = Instantiate(dangerZoneConePrefab, spawnPos, transform.rotation);
            currentSkillDangerZone.transform.SetParent(transform);
            currentSkillDangerZone.transform.localRotation = dangerZoneConePrefab.transform.localRotation;
            currentSkillDangerZone.transform.localScale = dangerZoneConePrefab.transform.localScale;
        }

        anim.SetTrigger("skill2");
        yield return new WaitForSeconds(skill2TelegraphDuration);

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= skill2Range && angleToPlayer <= (skill2Angle / 2f) && !isDead)
        {
            player.GetComponent<Health>().TakeDamage(skill2InstantDamage);
        }

        if (iceFloorPrefab != null && !isDead)
        {
            Vector3 spawnIcePos;
            Quaternion spawnIceRot;

            if (iceSpawnPoint != null)
            {
                spawnIcePos = iceSpawnPoint.position;
                spawnIceRot = iceSpawnPoint.rotation;
            }
            else
            {
                spawnIcePos = new Vector3(targetFloorPosition.x, targetFloorPosition.y + 0.02f, targetFloorPosition.z);
                spawnIceRot = Quaternion.identity;
            }

            GameObject icePatch = Instantiate(iceFloorPrefab, spawnIcePos, spawnIceRot);
            Destroy(icePatch, icePatchDuration); 
        }

        yield return new WaitForSeconds(0.8f);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill2Time = Time.time;
        isUsingSkill = false;
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
            activeShield.transform.SetParent(spawnPoint);
            activeShield.transform.localPosition = Vector3.zero;
            activeShield.transform.localRotation = Quaternion.identity;
        }

        if (dangerZoneCounterCirclePrefab != null)
        {
            Vector3 dangerPos = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
            currentSkillDangerZone = Instantiate(dangerZoneCounterCirclePrefab, dangerPos, Quaternion.identity);
            currentSkillDangerZone.transform.SetParent(this.transform);
            currentSkillDangerZone.transform.localScale = new Vector3(counterBlastRange * 2f, 0.1f, counterBlastRange * 2f);
        }

        yield return new WaitForSeconds(defendDuration);

        if (activeShield != null) Destroy(activeShield);
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone); 
        SetInvulnerable(false); 

        if (isDead) yield break;

        anim.SetTrigger("counterBlast"); 

        if (instantBlastVFX != null)
        {
            GameObject blastVFX = Instantiate(instantBlastVFX, transform.position, Quaternion.identity);
            Destroy(blastVFX, 2.5f); 
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= counterBlastRange)
        {
            player.GetComponent<Health>().TakeDamage(counterBlastDamage);
            
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.ApplyFreeze(freezeDuration);
            }
            
            Debug.Log("<color=blue>❄️ [WizardBoss Counter]: Player Terkena Ledakan Instan & Status FREEZE!</color>");
        }

        yield return new WaitForSeconds(0.8f); 

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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, counterBlastRange);

        // 🌟 GIZMOS BARU: RENDERING KOTAK MEMANJANG UNTUK AREA SKILL 1
        Matrix4x4 skill1Matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Color.magenta;
        Vector3 localCenter = new Vector3(0f, 0f, skill1Range / 2f);
        Vector3 localSize = new Vector3(skill1Width, 0.1f, skill1Range);
        Gizmos.DrawWireCube(localCenter, localSize);
        Gizmos.matrix = skill1Matrix;

        // Gizmos Area Kipas (Skill 2)
        Gizmos.color = Color.blue;
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