using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class DarkAlexander : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 20f; 
    public float attackRange = 3.5f; 
    public float moveSpeed = 4f;
    public float damage = 25f;
    public float attackCooldown = 2f;

    [Header("Animation Sync Settings")]
    [Tooltip("Makin kecil angkanya, gerakan kaki animasi lari akan semakin cepat. Sesuaikan sampai pas dengan visualnya.")]
    public float animSpeedMultiplier = 3.5f; 

    [Header("Attack Delay Settings")]
    public float attackDelay = 0.8f; 
    public float attackRecoveryTime = 0.4f;

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
    public GameObject skill1DangerZoneCirclePrefab;

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

    [Header("Skill 3 Settings (Meteor Rain - DOT)")]
    public float skill3Cooldown = 12f;     
    public float skill3Radius = 5.5f;           // Radius area lingkaran meteor
    public float skill3DamagePerTick = 8f;      // Damage yang diterima player tiap detak (tick)
    public float skill3TelegraphDuration = 1.2f;// Durasi lampu merah/indikator sebelum meteor jatuh
    public float meteorDuration = 5.0f;         // Berapa lama prefab meteor aktif di map
    public float tickInterval = 0.5f;           // Jeda waktu antar damage (misal: tiap 0.5 detik)
    public float meteorSpawnDistance = 4.0f;    // Jarak kemunculan meteor di depan bos (jika spawn point kosong)

    [Header("Skill 3 Prefabs & Points")]
    public GameObject skill3DangerZoneCirclePrefab; 
    public Transform skill3DangerZoneSpawnPoint;   // Titik pusat lingkaran meteor (Bisa dikosongkan)
    public GameObject meteorVFXPrefab;  // Indikator lingkaran merah untuk area meteor

    [Header("Counter Attack Settings (Defend & Jump Slam)")]
    public GameObject shieldPrefab;          
    public Transform shieldSpawnPoint;       
    public GameObject dangerZoneCirclePrefab; 
    public GameObject slamVFXPrefab;         
    public float slamRadius = 6f;             
    public float slamDamage = 45f;            
    public float defendDuration = 2f;        
    public float jumpDuration = 1.2f;         
    public float jumpHeight = 5f;             

    [Header("Counter Attack - Fire Zone DOT Settings")]
    public GameObject slamFireZonePrefab;       // Prefab efek lantai api membara
    public float fireZoneDuration = 5.0f;        // Berapa lama api bertahan di tanah
    public float fireZoneDamagePerTick = 6f;     // Damage bakar berkala
    public float fireZoneTickInterval = 0.5f;    // Jeda antar damage terbakar
    public float fireZoneRadius = 5f;           // Radius area kobaran api
    public float meteorImpactDelay = 0.5f;

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
    private float lastSkill3Time;         // Waktu peluncuran skill 3 terakhir

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
        }

        UpdateInvulnerabilityState(); 
        
        lastSkill1Time = Time.time - (skill1Cooldown / 2f); 
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); 
        lastSkill3Time = Time.time - (skill3Cooldown / 4f); 
    }

    void Update()
    {
        if (isDead) return;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh && agent.velocity.magnitude > 0.1f)
        {
            float calculatedSpeed = agent.velocity.magnitude / animSpeedMultiplier;
            anim.speed = Mathf.Max(1f, calculatedSpeed); 
        }
        else
        {
            anim.speed = 1f; 
        }

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

        float actualSkill3CheckDistance = skill3DangerZoneSpawnPoint != null ? 
            Vector3.Distance(skill3DangerZoneSpawnPoint.position, player.position) : (meteorSpawnDistance + skill3Radius);

        if (distanceToPlayer <= skill1Range && Time.time >= lastSkill1Time + skill1Cooldown)
        {
            StartCoroutine(Skill1Routine());
        }
        else if (distanceToPlayer <= skill2Range && Time.time >= lastSkill2Time + skill2Cooldown)
        {
            StartCoroutine(Skill2Routine());
        }
        else if (distanceToPlayer <= actualSkill3CheckDistance && Time.time >= lastSkill3Time + skill3Cooldown) 
        {
            StartCoroutine(Skill3Routine());
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
        Debug.LogWarning("INTERRUPT: Alexander memotong semua aksi untuk mengaktifkan Counter Attack!");

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
            anim.ResetTrigger("skill3"); 
            anim.ResetTrigger("defend");
            anim.ResetTrigger("jumpSlam"); 
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
        StopMoving();

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

        yield return new WaitForSeconds(attackRecoveryTime);
        lastAttackTime = Time.time;
        isPreparingAttack = false;
    }

    IEnumerator Skill1Routine()
    {
        isUsingSkill = true;
        StopMoving();

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        
        if (skill1DangerZoneCirclePrefab != null)
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z);
        currentSkillDangerZone = Instantiate(skill1DangerZoneCirclePrefab, spawnPos, Quaternion.identity);
        
        // Ukuran diatur secara radial menggunakan rumus diameter (Radius * 2) sesuai skill1Range
        currentSkillDangerZone.transform.localScale = new Vector3(skill1Range * 2f, 1f, skill1Range * 2f);
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

        Vector3 spawnPosition = transform.position + transform.forward * 1.5f;
        if (slashSpawnPoint != null)
        {
            spawnPosition = slashSpawnPoint.position;
        }
        else
        {
            spawnPosition.y += 1.0f; 
        }

        int projectileCount = 8;
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = i * angleStep;
            // Mengombinasikan arah hadap dasar boss dengan kalkulasi perputaran sudut
            Quaternion spawnRotation = transform.rotation * Quaternion.Euler(0f, currentAngle, 0f);
            Vector3 direction = spawnRotation * Vector3.forward;

            if (straightSlashPrefab != null)
            {
                GameObject slashVFX = Instantiate(straightSlashPrefab, spawnPosition, spawnRotation);

                Rigidbody rb = slashVFX.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * slashSpeed;
                }

                Destroy(slashVFX, 2.0f);
            }
        }

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);
    }

    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        StopMoving();

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

                Vector3 forwardNoY = transform.forward; forwardNoY.y = 0;

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

    IEnumerator Skill3Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger("skill3"); 

        int totalMeteors = 5;
        float delayBetweenMeteors = 0.4f; // Jeda waktu peluncuran antar tiap meteor strike

        for (int i = 0; i < totalMeteors; i++)
        {
            if (isDead) break;

            // Mengincar posisi absolut player pada frame loop berjalan
            Vector3 dynamicTargetPos = player.position;
            dynamicTargetPos.y += 0.03f; 

            // Eksekusi siklus hidup per meteor secara paralel agar tidak mengunci pergerakan boss
            StartCoroutine(ExecuteIndividualMeteorStrike(dynamicTargetPos));

            yield return new WaitForSeconds(delayBetweenMeteors);
        }

        // Boss bisa langsung kembali mengejar/menyerang sesaat setelah kelima meteor selesai dirapalkan
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        isUsingSkill = false; 
        lastSkill3Time = Time.time;
    }

    // Coroutine pembantu untuk mengurus siklus hidup mandiri tiap objek meteor tunggal
   IEnumerator ExecuteIndividualMeteorStrike(Vector3 targetPos)
    {
        // 1. Jalankan Indikator Danger Zone (Lampu Merah)
        GameObject localDangerZone = null;
        if (skill3DangerZoneCirclePrefab != null)
        {
            localDangerZone = Instantiate(skill3DangerZoneCirclePrefab, targetPos, Quaternion.identity);
            localDangerZone.transform.localScale = new Vector3(skill3Radius * 2f, 1f, skill3Radius * 2f);
        }

        // Tunggu sampai durasi lampu indikator selesai
        yield return new WaitForSeconds(skill3TelegraphDuration); 

        // Hancurkan indikator tepat saat meteor muncul dari langit
        if (localDangerZone != null) Destroy(localDangerZone);

        // 2. Spawn Prefab Visual Meteor (Meteor mulai meluncur jatuh)
        GameObject activeMeteorVFX = null;
        if (meteorVFXPrefab != null)
        {
            activeMeteorVFX = Instantiate(meteorVFXPrefab, targetPos, Quaternion.identity);
        }

        // 🔥 SINKRONISASI DI SINI:
        // Menahan script agar TIDAK memberikan damage dulu selama meteor masih melayang di udara
        yield return new WaitForSeconds(meteorImpactDelay);

        // 3. Mulai Perulangan Damage (DoT) setelah meteor dipastikan menghantam tanah
        float elapsed = 0f;
        while (elapsed < meteorDuration && !isDead)
        {
            Collider[] hitColliders = Physics.OverlapSphere(targetPos, skill3Radius);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    Health playerHealth = col.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(skill3DamagePerTick);
                    }
                }
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        if (activeMeteorVFX != null) Destroy(activeMeteorVFX);
    }

    // ========== MEKANIK COUNTER ATTACK JUMP SLAM ==========
    IEnumerator CounterAttackRoutine()
    {
        isCounterAttacking = true;
        isUsingSkill = true; 
        StopMoving();

        isCurrentlyDefending = true;
        UpdateInvulnerabilityState();
        nextHPThreshold -= health.maxHealth * 0.3f;

        Debug.Log("<color=blue>🛡️ ALEXANDER: Memasuki Fase Defend Stance!</color>");
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
        }

        yield return new WaitForSeconds(defendDuration);

        if (activeShield != null) Destroy(activeShield);
        isCurrentlyDefending = false;
        UpdateInvulnerabilityState(); 

        Debug.Log("<color=red>🚀 ALEXANDER: Melompat Menyerang Player (Jump Slam)!</color>");
        anim.SetTrigger("jumpSlam"); 

        Vector3 targetLandingPos = player.position;
        Vector3 startJumpPos = transform.position;

        if (dangerZoneCirclePrefab != null)
        {
            Vector3 dangerPos = new Vector3(targetLandingPos.x, targetLandingPos.y + 0.05f, targetLandingPos.z);
            currentSkillDangerZone = Instantiate(dangerZoneCirclePrefab, dangerPos, Quaternion.identity);
            currentSkillDangerZone.transform.localScale = new Vector3(slamRadius * 2f, 0.1f, slamRadius * 2f);
        }

        if (agent.isActiveAndEnabled) agent.enabled = false;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            if (isDead) break;

            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / jumpDuration;

            Vector3 currentXZ = Vector3.Lerp(startJumpPos, targetLandingPos, normalizedTime);
            float arcHeight = Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;
            float currentY = Mathf.Lerp(startJumpPos.y, targetLandingPos.y, normalizedTime) + arcHeight;

            transform.position = new Vector3(currentXZ.x, currentY, currentXZ.z);

            Vector3 lookDir = targetLandingPos - startJumpPos;
            lookDir.y = 0;
            if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);

            yield return null;
        }

        if (!isDead)
        {
            transform.position = targetLandingPos; 
        }

        yield return new WaitForSeconds(0.8f); 

        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.speed = moveSpeed;
                agent.isStopped = false;
            }
        }

        isUsingSkill = false;
        isCounterAttacking = false;
        Debug.Log("<color=green>✅ ALEXANDER: Fase Counter Attack Selesai.</color>");
    }

    public void TriggerJumpSlamImpact()
    {
        if (isDead) return;

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        if (slamVFXPrefab != null)
        {
            currentSkillVFX = Instantiate(slamVFXPrefab, transform.position, Quaternion.identity);
            Destroy(currentSkillVFX, 2f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, slamRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                Health playerHealth = col.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(slamDamage);
                }
            }
        }

        StartCoroutine(ExecuteSlamFireZoneRoutine(transform.position));
    }

    // Coroutine independen untuk mengurusi damage bakar berkala di area bekas pendaratan
    IEnumerator ExecuteSlamFireZoneRoutine(Vector3 spawnPos)
    {
        GameObject fireZoneVFX = null;
        if (slamFireZonePrefab != null)
        {
            Vector3 fireSpawnPos = new Vector3(spawnPos.x, spawnPos.y + 0.02f, spawnPos.z);
            fireZoneVFX = Instantiate(slamFireZonePrefab, fireSpawnPos, Quaternion.identity);
            fireZoneVFX.transform.localScale = new Vector3(fireZoneRadius * 2f, 1f, fireZoneRadius * 2f);
        }

        float elapsed = 0f;
        while (elapsed < fireZoneDuration && !isDead)
        {
            Collider[] hitColliders = Physics.OverlapSphere(spawnPos, fireZoneRadius);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    Health playerHealth = col.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(fireZoneDamagePerTick);
                        Debug.Log("<color=orange>🔥 Player terbakar di area bekas Jump Slam! Terkena " + fireZoneDamagePerTick + " Damage DoT.</color>");
                    }
                }
            }

            yield return new WaitForSeconds(fireZoneTickInterval);
            elapsed += fireZoneTickInterval;
        }

        // Menghapus efek visual api secara otomatis begitu durasi habis
        if (fireZoneVFX != null) Destroy(fireZoneVFX);
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

    // --- SISA KODE GIZMOS TETAP SAMA ---
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, slamRadius);

        Gizmos.color = new Color(1f, 0f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, skill1Range);

        Vector3 expectedMeteorPos = skill3DangerZoneSpawnPoint != null ? 
            skill3DangerZoneSpawnPoint.position : (transform.position + transform.forward * meteorSpawnDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(expectedMeteorPos, skill3Radius);

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