using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class KingBarbarian : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 20f; 
    public float attackRange = 3.5f; 
    public float moveSpeed = 4f;
    public float damage = 25f;
    public float attackCooldown = 2f;

    [Header("Attack Delay Settings")]
    public float attackDelay = 0.8f; 

    [Header("Skill 1 Settings (Chop & Straight Slash)")]
    public float skill1Cooldown = 6f;
    public float skill1Range = 10f;
    public float slashSpeed = 12f;         
    public float skill1Width = 4f;         
    public float jumpDistance = 8f;        
    public float telegraphDuration = 1.0f; 
    public float postHitDuration = 0.8f;   

    [Header("Skill 1 Prefabs & Visuals")]
    public GameObject straightSlashPrefab; 
    public Transform slashSpawnPoint;      
    public GameObject dangerZoneBoxPrefab; 

    [Header("Skill 2 Settings (Side Slash Semi-Circle)")]
    public float skill2Cooldown = 8f;
    public float skill2Range = 5f;          
    public float skill2Angle = 140f;        
    public float skill2Damage = 35f;        
    public float skill2TelegraphDuration = 1.0f; 
    public float skill2PostHitDuration = 0.8f;   

    [Header("Skill 2 Prefabs & Visuals")]
    public GameObject dangerZoneConePrefab; 
    public GameObject sideSlashPrefab;     
    public Transform sideSlashSpawnPoint;   

    [Header("Skill 3 Settings (Counter Attack - Defend & Spin Top)")]
    public GameObject shieldPrefab;          
    public Transform shieldSpawnPoint;       
    public GameObject dangerZoneCirclePrefab; 
    public GameObject spinVFXPrefab;         
    
    public float spinRange = 6f;             
    public float spinDamagePerTick = 12f;    
    public float defendDuration = 2f;        
    public float spinDuration = 5f;          
    public float spinMoveSpeed = 6.5f;       

    [Header("MECHANICS: Counter Attack Threshold")]
    private float nextHPThreshold;          
    private bool isCounterAttacking = false; 

    [Header("MECHANICS: Super Armor Linked Boss")]
    public Health otherBossHealth;           
    public GameObject superArmorVisualVFX;   

    private bool hasSuperArmor = false;       
    private bool isCurrentlyDefending = false; 
    private GameObject activeSuperArmorInstance; 

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

        if (otherBossHealth != null)
        {
            hasSuperArmor = true;
            otherBossHealth.onDeath += RemoveSuperArmor; 
            
            if (superArmorVisualVFX != null) 
            {
                Transform spawnPoint = shieldSpawnPoint != null ? shieldSpawnPoint : transform;
                activeSuperArmorInstance = Instantiate(superArmorVisualVFX, spawnPoint.position, spawnPoint.rotation);
                activeSuperArmorInstance.transform.SetParent(spawnPoint);
                activeSuperArmorInstance.transform.localPosition = Vector3.zero;
                activeSuperArmorInstance.transform.localRotation = Quaternion.identity;
            }
            
            Debug.Log("<color=purple>🛡️ KING BARBARIAN: Super Armor AKTIF. Kalahkan bos sebelah terlebih dahulu!</color>");
        }

        UpdateInvulnerabilityState(); 
        
        lastSkill1Time = Time.time - (skill1Cooldown / 2f); 
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); 
    }

    void Update()
    {
        if (isDead) return;

        // ====================================================================
        // 🔥 FAIL-SAFE SYSTEM FOR SUPER ARMOR
        // Jika perisai masih aktif tapi Stone Dragon terdeteksi hancur (null) ATAU HP-nya sudah 0,
        // langsung paksa hancurkan Super Armor tanpa menunggu kiriman event.
        // ====================================================================
        if (hasSuperArmor)
        {
            if (otherBossHealth == null || otherBossHealth.currentHealth <= 0)
            {
                RemoveSuperArmor();
            }
        }

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
        Debug.LogWarning("INTERRUPT: KingBarbarian memotong semua aksi untuk mengaktifkan Counter Attack!");

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
            anim.ResetTrigger("spinAttack");
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
            if (player.GetComponent<Health>() != null)
            {
                player.GetComponent<Health>().TakeDamage(damage);
            }
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
        
        if (dangerZoneBoxPrefab != null)
        {
            Vector3 boxCenter = transform.position + transform.forward * (jumpDistance / 2f);
            Vector3 spawnPos = new Vector3(boxCenter.x, transform.position.y + 0.02f, boxCenter.z);
            currentSkillDangerZone = Instantiate(dangerZoneBoxPrefab, spawnPos, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            currentSkillDangerZone.transform.localScale = new Vector3(skill1Width, jumpDistance, 1f);
        }

        anim.SetTrigger("skill1");

        yield return new WaitForSeconds(telegraphDuration);
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        yield return new WaitForSeconds(postHitDuration);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill1Time = Time.time;
        isUsingSkill = false;
    }

    public void TriggerChopSlash()
    {
        if (isDead) return;

        if (straightSlashPrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
            Quaternion spawnRotation = transform.rotation;

            if (slashSpawnPoint != null)
            {
                spawnPosition = slashSpawnPoint.position;
                spawnRotation = slashSpawnPoint.rotation;
            }
            else
            {
                spawnPosition.y += 1.0f; 
            }

            GameObject slashVFX = Instantiate(straightSlashPrefab, spawnPosition, spawnRotation);

            Rigidbody rb = slashVFX.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * slashSpeed;
            }

            Destroy(slashVFX, 2.0f);
        }

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);
    }

    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (dangerZoneConePrefab != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 0.03f, transform.position.z);
            currentSkillDangerZone = Instantiate(dangerZoneConePrefab, spawnPos, transform.rotation);
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
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(skill2Damage);
                    }
                }
            }
        }
    }

    IEnumerator CounterAttackRoutine()
    {
        isCounterAttacking = true;
        isUsingSkill = true; 
        StopMoving();

        isCurrentlyDefending = true;
        UpdateInvulnerabilityState();
        nextHPThreshold -= health.maxHealth * 0.3f;

        Debug.Log("<color=blue>🛡️ KING BARBARIAN: Memasuki Fase Defend Stance!</color>");
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
            currentSkillDangerZone.transform.localScale = new Vector3(spinRange * 2f, 0.1f, spinRange * 2f);
        }

        yield return new WaitForSeconds(defendDuration);

        if (activeShield != null) Destroy(activeShield);
        isCurrentlyDefending = false;
        UpdateInvulnerabilityState(); 

        Debug.Log("<color=red>🌀 KING BARBARIAN: Memasuki Fase Spin Attack (Mengejar Player)!</color>");
        anim.SetTrigger("spinAttack"); 

        if (spinVFXPrefab != null)
        {
            Transform spawnPoint = shieldSpawnPoint != null ? shieldSpawnPoint : transform;
            currentSkillVFX = Instantiate(spinVFXPrefab, spawnPoint.position, spawnPoint.rotation);
            currentSkillVFX.transform.SetParent(spawnPoint);
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = spinMoveSpeed;
        }

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            if (isDead) break;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
                anim.SetBool("isMoving", true);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= spinRange)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(spinDamagePerTick);
                    Debug.Log("<color=red>💥 Player Terkena Tebasan Berputar! Damage: </color>" + spinDamagePerTick);
                }
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

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = moveSpeed;
            agent.isStopped = false;
        }

        isUsingSkill = false;
        isCounterAttacking = false;
        Debug.Log("<color=green>✅ KING BARBARIAN: Fase Counter Attack Selesai.</color>");
    }

    void RemoveSuperArmor()
    {
        hasSuperArmor = false;
        UpdateInvulnerabilityState();

        if (otherBossHealth != null)
        {
            otherBossHealth.onDeath -= RemoveSuperArmor; 
        }

        if (activeSuperArmorInstance != null) 
            Destroy(activeSuperArmorInstance);

        Debug.Log("<color=green>⚡ SHATTERED: Stone Dragon gugur! Super Armor King Barbarian telah HANCUR!</color>");
    }

    void UpdateInvulnerabilityState()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            bool shouldBeInvulnerable = hasSuperArmor || isCurrentlyDefending;
            col.enabled = !shouldBeInvulnerable;
        }
    }

    void HandleBossDeath()
    {
        isDead = true;
        StopAllCoroutines();
        
        hasSuperArmor = false;
        isCurrentlyDefending = false;
        UpdateInvulnerabilityState();
        
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);
        if (currentSkillVFX != null) Destroy(currentSkillVFX);
        if (activeSuperArmorInstance != null) Destroy(activeSuperArmorInstance);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (agent != null) agent.enabled = true; 
        if (health != null && health.healthSlider != null) Invoke("HideBossUI", 3f);
        
        this.enabled = false; 
    }

    void HideBossUI()
    {
        if (health != null && health.healthSlider != null) 
            health.healthSlider.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, spinRange);

        Matrix4x4 skill1Matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0f, 1f, 0.15f);
        Vector3 boxCenter = new Vector3(0f, 0f, jumpDistance / 2f);
        Vector3 boxSize = new Vector3(skill1Width, 0.1f, jumpDistance);
        Gizmos.DrawCube(boxCenter, boxSize);
        Gizmos.DrawWireCube(boxCenter, boxSize);
        Gizmos.matrix = skill1Matrix;

        Gizmos.color = Color.orange;
        Vector3 leftBoundary = Quaternion.Euler(0, -skill2Angle / 2f, 0) * transform.forward * skill2Range;
        Vector3 rightBoundary = Quaternion.Euler(0, skill2Angle / 2f, 0) * transform.forward * skill2Range;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        int segments = 15;
        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -skill2Angle / 2f + (skill2Angle / segments) * i;
            Vector3 nextPoint = transform.position + Quaternion.Euler(0, currentAngle, 0) * transform.forward * skill2Range;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}