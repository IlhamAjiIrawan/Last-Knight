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
    public float skill2TelegraphDuration = 1.0f; 
    public float skill2Duration = 2.5f;    

    [Header("Danger Zone Visuals")]
    public GameObject dangerZoneBoxPrefab;   
    public GameObject dangerZoneConePrefab;  

    [Header("Skill 2 VFX Prefabs")]
    public GameObject fireBreathPrefab;      
    public Transform fireSpawnPoint;         

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

    [Header("Ultimate Skill Settings")]
    public float ultCooldown = 25f;           
    public float ultDuration = 4f;            
    public float ultTelegraphDuration = 1.8f; 
    public float ultFlyHeight = 18f;          
    public float ultWidth = 8f;               
    public float ultLength = 14f;              
    public float ultDamagePerTick = 20f;      
    public GameObject dangerZoneSquarePrefab; 
    public GameObject ultFirePrefab;          

    [Header("Ultimate Spawn Control")]
    public Transform ultimateSpawnPoint;      
    public Transform ultimateDangerZonePoint; 
    public Vector3 ultFireOffset;             
    
    [HideInInspector] public bool spawnFromBossInSky = true; 
    [HideInInspector] public Transform ultFireSpawnPoint;

    private float lastUltTime;
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
        lastSkill2Time = Time.time - (skill2Cooldown / 3f); 
        lastUltTime = Time.time - (ultCooldown - 10f); 

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

        if (distanceToPlayer <= chaseRange && Time.time >= lastUltTime + ultCooldown)
        {
            StartCoroutine(UltimateSkillRoutine());
        }
        else if (distanceToPlayer <= skill1Range && Time.time >= lastSkill1Time + skill1Cooldown)
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
        Debug.LogWarning("INTERRUPT: Boss memotong semua aksi untuk mengaktifkan Counter Attack!");

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
            anim.ResetTrigger("takeOff");
            anim.ResetTrigger("land");
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

    IEnumerator Skill2Routine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (dangerZoneConePrefab != null)
        {
            Vector3 spawnPos = fireSpawnPoint != null ? fireSpawnPoint.position : transform.position;
            spawnPos.y = transform.position.y + 0.05f;

            currentSkillDangerZone = Instantiate(dangerZoneConePrefab, spawnPos, transform.rotation);
            currentSkillDangerZone.transform.SetParent(transform);
            currentSkillDangerZone.transform.localRotation = dangerZoneConePrefab.transform.localRotation;
            currentSkillDangerZone.transform.localScale = dangerZoneConePrefab.transform.localScale;
        }

        anim.SetTrigger("skill2");
        yield return new WaitForSeconds(skill2TelegraphDuration);

        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        if (fireBreathPrefab != null)
        {
            Transform spawnPoint = fireSpawnPoint != null ? fireSpawnPoint : transform;
            currentSkillVFX = Instantiate(fireBreathPrefab, spawnPoint.position, spawnPoint.rotation);
            currentSkillVFX.transform.SetParent(spawnPoint);
        }

        float elapsed = 0f;
        while (elapsed < skill2Duration)
        {
            if (isDead) break;

            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= skill2Range && angleToPlayer <= (skill2Angle / 2f))
            {
                player.GetComponent<Health>().TakeDamage(skill2DamagePerTick);
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
            // Menggunakan shieldSpawnPoint (atau transform utama jika kosong) sebagai titik pusat angin
            Transform spawnPoint = shieldSpawnPoint != null ? shieldSpawnPoint : transform;
            currentSkillVFX = Instantiate(windPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Menjadikan anak dari boss agar efek angin ikut berputar mengikuti arah hadap boss ke player
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
                Destroy(currentSkillVFX, 2f); // Beri jeda 2 detik agar sisa partikel menghilang halus
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

    IEnumerator UltimateSkillRoutine()
    {
        isUsingSkill = true;
        anim.SetBool("isMoving", false);
        
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = true;
        agent.enabled = false;

        Debug.LogWarning("ULTIMATE: Boss Lepas Landas Menuju Langit!");
        anim.SetTrigger("takeOff"); 

        Vector3 startGroundPos = transform.position;
        Vector3 targetFlyPos = startGroundPos + Vector3.up * ultFlyHeight;

        float flyElapsed = 0f;
        float flyDuration = 1.2f;
        while (flyElapsed < flyDuration)
        {
            if (isDead) break;
            transform.position = Vector3.Lerp(startGroundPos, targetFlyPos, flyElapsed / flyDuration);
            flyElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetFlyPos;

        Vector3 targetAttackCenter;
        Quaternion attackRotation;

        if (ultimateDangerZonePoint != null)
        {
            targetAttackCenter = ultimateDangerZonePoint.position;
            attackRotation = ultimateDangerZonePoint.rotation;
        }
        else
        {
            targetAttackCenter = new Vector3(player.position.x, startGroundPos.y, player.position.z);
            Vector3 lookDir = (targetAttackCenter - new Vector3(transform.position.x, startGroundPos.y, transform.position.z)).normalized;
            attackRotation = Quaternion.LookRotation(lookDir);
        }

        if (dangerZoneSquarePrefab != null)
        {
            Vector3 spawnZonePos = new Vector3(targetAttackCenter.x, targetAttackCenter.y + 0.02f, targetAttackCenter.z);
            Quaternion spriteRotation = attackRotation * Quaternion.Euler(90f, 0f, 0f);
            currentSkillDangerZone = Instantiate(dangerZoneSquarePrefab, spawnZonePos, spriteRotation);
            currentSkillDangerZone.transform.localScale = new Vector3(ultWidth, ultLength, 1f);
        }

        yield return new WaitForSeconds(ultTelegraphDuration);
        if (currentSkillDangerZone != null) Destroy(currentSkillDangerZone);

        if (ultFirePrefab != null)
        {
            Vector3 fireSpawnPos;
            Quaternion fireRotation;

            if (ultimateSpawnPoint != null)
            {
                fireSpawnPos = ultimateSpawnPoint.position + ultimateSpawnPoint.TransformDirection(ultFireOffset);
                fireRotation = ultimateSpawnPoint.rotation;
            }
            else
            {
                fireSpawnPos = new Vector3(targetAttackCenter.x, targetAttackCenter.y + 0.1f, targetAttackCenter.z) + ultFireOffset;
                fireRotation = attackRotation;
            }

            currentSkillVFX = Instantiate(ultFirePrefab, fireSpawnPos, fireRotation);
            
            if (ultimateSpawnPoint != null)
            {
                currentSkillVFX.transform.SetParent(ultimateSpawnPoint);
            }
        }

        float attackElapsed = 0f;
        Vector3 boxHalfExtents = new Vector3(ultWidth / 2f, 6f, ultLength / 2f);
        while (attackElapsed < ultDuration)
        {
            if (isDead) break;

            Collider[] hitColliders = Physics.OverlapBox(targetAttackCenter, boxHalfExtents, attackRotation);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    col.GetComponent<Health>().TakeDamage(ultDamagePerTick);
                    break; 
                }
            }
            yield return new WaitForSeconds(0.5f);
            attackElapsed += 0.5f;
        }

        if (currentSkillVFX != null) Destroy(currentSkillVFX);

        Debug.LogWarning("ULTIMATE: Boss Kembali Mendarat ke Arena!");
        anim.SetTrigger("land"); 

        Vector3 airPos = transform.position;
        Vector3 landTargetPos = new Vector3(airPos.x, startGroundPos.y, airPos.z); 

        float landElapsed = 0f;
        float landDuration = 1.2f;
        while (landElapsed < landDuration)
        {
            if (isDead) break;
            transform.position = Vector3.Lerp(airPos, landTargetPos, landElapsed / landDuration);
            landElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = landTargetPos;

        agent.enabled = true;
        yield return null; 
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.isStopped = false;

        lastUltTime = Time.time;
        isUsingSkill = false;
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

        Gizmos.color = Color.cyan;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(ultWidth, 0.5f, ultLength));
        Gizmos.matrix = originalMatrix;

        Matrix4x4 skill1Matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Vector3 localCenter = new Vector3(0f, 0f, jumpDistance / 2f);
        Vector3 localSize = new Vector3(skill1Width, 0.1f, jumpDistance);
        Gizmos.DrawCube(localCenter, localSize);
        Gizmos.DrawWireCube(localCenter, localSize);
        Gizmos.matrix = skill1Matrix;

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