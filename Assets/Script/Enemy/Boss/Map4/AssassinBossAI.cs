using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class AssassinBossAI : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 22f; 
    public float attackRange = 2.5f; 
    public float moveSpeed = 6f; 
    public float damage = 18f;
    public float attackCooldown = 1.5f;

    [Header("Attack Delay Settings")]
    public float attackDelay = 0.4f; 

    [Header("Skill 1 Settings (Shadow Dash Strike - Box Area)")]
    public float skill1Cooldown = 7f;
    public float skill1Range = 12f;          
    public float dashSpeed = 18f;         
    public float skill1Width = 3f;           
    public float telegraphDuration = 0.8f; 
    public float postHitDuration = 0.5f;   

    [Header("Skill 1 Prefabs & Visuals")]
    public GameObject dashSlashPrefab;      
    public Transform slashSpawnPoint;      
    public GameObject dangerZoneBoxPrefab;  

    [Header("Skill 2 Settings (Quick Knife Throw - Ranged)")]
    public float skill2Cooldown = 5f;
    public float skill2Range = 14f;          // Jarak lemparan pisau (lebih jauh dari dash)
    public int knivesCount = 3;             // Jumlah pisau yang dilempar berturut-turut
    public float throwInterval = 0.2f;       // Jeda antar lemparan pisau (makin kecil makin cepat)
    public float knifeSpeed = 22f;
    public float knifeDamage = 12f;
    public GameObject knifePrefab;          // Prefab Proyektil Pisau (Beri Rigidbody + Collider)
    public Transform knifeSpawnPoint;       // Titik lepas pisau (misal: di posisi tangan bos)

    [Header("Skill 3 Settings (Counter Attack - Smoke Bomb & Blade Flurry)")]
    public GameObject smokeBombPrefab;       
    public Transform smokeSpawnPoint;       
    public GameObject dangerZoneCirclePrefab; 
    public GameObject flurryVFXPrefab;       
    
    public float flurryRange = 5f;             
    public float flurryDamagePerTick = 15f;    
    public float vanishDuration = 1.5f;       
    public float flurryDuration = 4f;          
    public float flurryMoveSpeed = 8f;        

    [Header("MECHANICS: Counter Attack Threshold")]
    private float nextHPThreshold;          
    private bool isCounterAttacking = false; 
    private bool isCurrentlyVanish = false;   

    private NavMeshAgent agent;
    private Animator anim;
    private Health health;
    private bool isDead = false;
    private bool isPreparingAttack = false;
    private bool isUsingSkill = false;    
    private float lastAttackTime;
    private float lastSkill1Time;         
    private float lastSkill2Time; // 🌟 Tracking cooldown skill 2        

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

        UpdateInvulnerabilityState(); 
        
        // Mengatur cooldown awal agar tidak langsung spam skill di detik pertama game
        lastSkill1Time = Time.time - (skill1Cooldown / 2f); 
        lastSkill2Time = Time.time - (skill2Cooldown / 2f); 
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

        // 🌟 JALUR LOGIKA AI: Cek Skill 1 -> Cek Skill 2 -> Cek Melee Attack -> Chase
        if (distanceToPlayer <= skill1Range && Time.time >= lastSkill1Time + skill1Cooldown)
        {
            StartCoroutine(Skill1Routine());
        }
        else if (distanceToPlayer <= skill2Range && Time.time >= lastSkill2Time + skill2Cooldown)
        {
            StartCoroutine(Skill2Routine()); // 🌟 Eksekusi Lempar Pisau
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
        Debug.LogWarning("INTERRUPT: Assassin Boss memotong semua aksi untuk mengaktifkan Counter Attack!");

        StopAllCoroutines();
        SetBossMeshVisible(true); // Fail-safe agar tubuh muncul kembali jika terinterupsi

        if (agent != null && !agent.enabled)
        {
            Vector3 groundPos = transform.position;
            groundPos.y = player.position.y; 
            transform.position = groundPos;
            agent.enabled = true;
        }

        isPreparingAttack = false;
        isUsingSkill = false;
        isCurrentlyVanish = false;
        UpdateInvulnerabilityState();

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);
        if (currentSkillVFX != null) Destroy(currentSkillVFX);

        if (anim != null)
        {
            anim.ResetTrigger("attack");
            anim.ResetTrigger("skill1");
            anim.ResetTrigger("skill2");
            anim.ResetTrigger("vanish"); 
            anim.ResetTrigger("flurryAttack"); 
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

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f && !isDead)
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

        LookAtPlayer();
        
        if (dangerZoneBoxPrefab != null)
        {
            Vector3 boxCenter = transform.position + transform.forward * (skill1Range / 2f);
            Vector3 spawnPos = new Vector3(boxCenter.x, transform.position.y + 0.02f, boxCenter.z);
            currentSkillDangerZone = Instantiate(dangerZoneBoxPrefab, spawnPos, transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            currentSkillDangerZone.transform.localScale = new Vector3(skill1Width, skill1Range, 1f);
        }

        anim.SetTrigger("skill1");

        yield return new WaitForSeconds(telegraphDuration);
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        TriggerShadowDash();

        yield return new WaitForSeconds(postHitDuration);

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill1Time = Time.time;
        isUsingSkill = false;
    }

    public void TriggerShadowDash()
    {
        if (isDead) return;

        Vector3 dashTarget = transform.position + transform.forward * skill1Range;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dashTarget, out hit, 3f, NavMesh.AllAreas))
        {
            dashTarget = hit.position;
        }

        if (dashSlashPrefab != null)
        {
            Vector3 vfxPos = slashSpawnPoint != null ? slashSpawnPoint.position : transform.position + Vector3.up;
            GameObject vfx = Instantiate(dashSlashPrefab, vfxPos, transform.rotation);
            Destroy(vfx, 1.5f);
        }

        transform.position = dashTarget;

        Collider[] hitPlayers = Physics.OverlapBox(transform.position - transform.forward * (skill1Range / 2f), new Vector3(skill1Width / 2f, 2f, skill1Range / 2f), transform.rotation);
        foreach (Collider col in hitPlayers)
        {
            if (col.CompareTag("Player"))
            {
                Health playerHealth = col.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage * 1.5f);
                }
            }
        }
    }

    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        LookAtPlayer();
        anim.SetTrigger("skill2");

        // Delay singkat menunggu animasi tangan bos berayun ke depan sebelum pisau pertama keluar
        yield return new WaitForSeconds(0.25f); 

        for (int i = 0; i < knivesCount; i++)
        {
            if (isDead || isCounterAttacking) break;

            LookAtPlayer(); // Tetap lock target ke arah posisi terbaru player saat melempar
            SpawnKnifeProjectile();

            yield return new WaitForSeconds(throwInterval); // Jeda sebelum pisau berikutnya dilempar
        }

        yield return new WaitForSeconds(0.4f); // Waktu recovery setelah selesai melempar semua pisau

        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;
        lastSkill2Time = Time.time;
        isUsingSkill = false;
    }

    void SpawnKnifeProjectile()
    {
        if (knifePrefab == null)
        {
            Debug.LogError("🚨 KESALAHAN: 'Knife Prefab' belum dimasukkan di Inspector Boss!");
            return;
        }

        Vector3 spawnPos = knifeSpawnPoint != null ? knifeSpawnPoint.position : transform.position + Vector3.up * 1.2f;
        Vector3 targetDirection = ((player.position + Vector3.up * 1.0f) - spawnPos).normalized;
        Quaternion projectileRotation = Quaternion.LookRotation(targetDirection);

        GameObject knifeGO = Instantiate(knifePrefab, spawnPos, projectileRotation);

        Rigidbody rb = knifeGO.GetComponentInChildren<Rigidbody>();
        
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false; // Memastikan paksa lewat code agar fisika aktif

            // ✨ FAIL-SAFE VERSI UNITY: Jika Unity kamu versi lama dan linearVelocity error, 
            // kamu bisa menggantinya dengan: rb.velocity = targetDirection * knifeSpeed;
            rb.linearVelocity = targetDirection * knifeSpeed;
            
            Debug.Log("🚀 PISAU BERHASIL DIDORONG! Kecepatan: " + knifeSpeed);
        }
        else
        {
            // Jika pesan ini muncul di Console, berarti letak Rigidbody kamu salah!
            Debug.LogError("❌ ERROR: Rigidbody TIDAK DITEMUKAN pada Prefab Pisau! Pisau tidak akan bisa terbang.");
        }

        // Cari script damage-nya
        KnifeProjectile projectileScript = knifeGO.GetComponentInChildren<KnifeProjectile>();
        if (projectileScript != null)
        {
            projectileScript.damage = knifeDamage;
            projectileScript.speed = knifeSpeed;
        }
    }

    IEnumerator CounterAttackRoutine()
    {
        isCounterAttacking = true;
        isUsingSkill = true; 
        StopMoving();

        isCurrentlyVanish = true;
        UpdateInvulnerabilityState();
        nextHPThreshold -= health.maxHealth * 0.3f; 

        Debug.Log("<color=purple>🌫️ ASSASSIN BOSS: Menggunakan Smoke Bomb & Menghilang (Kebal)!</color>");
        anim.SetTrigger("vanish"); 
        
        GameObject activeSmoke = null;
        if (smokeBombPrefab != null)
        {
            Transform spawnPoint = smokeSpawnPoint != null ? smokeSpawnPoint : transform;
            activeSmoke = Instantiate(smokeBombPrefab, spawnPoint.position, spawnPoint.rotation);
            activeSmoke.transform.SetParent(spawnPoint);
            activeSmoke.transform.localPosition = Vector3.zero;
        }

        if (dangerZoneCirclePrefab != null)
        {
            Vector3 dangerPos = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
            currentSkillDangerZone = Instantiate(dangerZoneCirclePrefab, dangerPos, Quaternion.identity);
            currentSkillDangerZone.transform.SetParent(this.transform);
            currentSkillDangerZone.transform.localScale = new Vector3(flurryRange * 2f, 0.1f, flurryRange * 2f);
        }

        yield return new WaitForSeconds(vanishDuration);

        if (activeSmoke != null) Destroy(activeSmoke);
        isCurrentlyVanish = false;
        UpdateInvulnerabilityState(); 

        Debug.Log("<color=red>🌀 ASSASSIN BOSS: Memulai Blade Flurry (Mengejar Cepat & MENGHILANG)!</color>");
        anim.SetTrigger("flurryAttack"); 

        SetBossMeshVisible(false); // Tubuh bos menghilang!

        if (flurryVFXPrefab != null)
        {
            Transform spawnPoint = smokeSpawnPoint != null ? smokeSpawnPoint : transform;
            currentSkillVFX = Instantiate(flurryVFXPrefab, spawnPoint.position, spawnPoint.rotation);
            currentSkillVFX.transform.SetParent(spawnPoint);
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = flurryMoveSpeed; 
        }

        float elapsed = 0f;
        while (elapsed < flurryDuration)
        {
            if (isDead) break;

            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
                anim.SetBool("isMoving", true);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= flurryRange)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(flurryDamagePerTick);
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
                Destroy(currentSkillVFX, 1.5f); 
            }
            else 
            { 
                Destroy(currentSkillVFX); 
            }
        }

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        SetBossMeshVisible(true); // Tubuh bos muncul kembali!

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = moveSpeed;
            agent.isStopped = false;
        }

        isUsingSkill = false;
        isCounterAttacking = false;
    }

    void UpdateInvulnerabilityState()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = !isCurrentlyVanish;
        }
    }

    void SetBossMeshVisible(bool visible)
    {
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in renderers)
        {
            smr.enabled = visible;
        }
    }

    void HandleBossDeath()
    {
        isDead = true;
        StopAllCoroutines();
        
        isCurrentlyVanish = false;
        UpdateInvulnerabilityState();
        SetBossMeshVisible(true); 
        
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
        if (health != null && health.healthSlider != null) 
            health.healthSlider.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.blue; // Visualisasi jarak tembak pisau di Editor
        Gizmos.DrawWireSphere(transform.position, skill2Range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, flurryRange);

        Matrix4x4 skill1Matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Color.magenta;
        Vector3 localCenter = new Vector3(0f, 0f, skill1Range / 2f);
        Vector3 localSize = new Vector3(skill1Width, 0.1f, skill1Range);
        Gizmos.DrawWireCube(localCenter, localSize);
        Gizmos.matrix = skill1Matrix;
    }
}