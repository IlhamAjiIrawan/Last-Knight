using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody rb;
    private Camera mainCamera;
    private Animator anim;
    public float attackDamage = 1f;
    public float attackRange = 1.5f;

    [Header("Attack Settings")]
    public float swingRange = 2f;
    public float heavyRange = 1.5f;
    [Range(0, 180)] public float swingAngle = 120f; // Area setengah lingkaran

    [Header("Block Settings")]
    public float maxBlockGauge = 5f;          // Batas maksimum pertahanan
    public float currentBlockGauge;             // Nilai pertahanan saat ini
    public float blockRegenRate = 1f;          // Kecepatan pulih pertahanan per detik
    public float blockSpeedMultiplier = 0.2f;   // Kecepatan gerak saat blok (0% dari speed normal)
    public bool isBlocking { get; private set; }
    public bool isBlockBroken { get; private set; } = false;

    [Header("UI Damage Flash Settings")]
    public Image damageFlashImage;         // Tarik UI Image merah full-screen ke sini di Inspector
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f); // Warna merah dengan transparansi 40%
    public float flashSpeed = 5f;
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;
    public float invincibilityDuration = 0.5f; // Durasi immune yang bisa kamu atur
    public bool isImmune { get; private set; }
    public bool isDashing { get; private set; }
    private float lastDashTime;

    [Header("Energy Costs")]
    public float dashEnergyCost = 5f;

    [Header("Potion Buff Status")]
    private bool isUnlimitedEnergy = false;
    private float potionDamageMultiplier = 1f;
    private float potionSpeedMultiplier = 1f;

    [HideInInspector] public float trapSpeedMultiplier = 1f;

    public Transform attackPoint;
    public LayerMask enemyLayers;
    public bool isUsingPotion = false;

    [Header("Rage Mode Settings")]
    public float rageDuration = 100f;
    public float rageAnimSpeed = 1.5f;

    [Header("VFX Settings")]
    public GameObject slashPrefabCombo1;
    public GameObject slashPrefabCombo2;
    public GameObject slashPrefabCombo3;
    public GameObject heavySlashPrefab;
    public Transform spawnPointCombo1;
    public Transform spawnPointCombo2;
    public Transform spawnPointCombo3;
    public Transform heavySpawnPoint;
    public GameObject impactVFXPrefab;

    [Header("Rage VFX Settings")]
    public GameObject rageAuraPrefab;
    private GameObject activeAura;
    
    [Header("Queue Combo Settings")]
    public int comboStep = 0;
    public int clickQueueCount = 0;
    public float knockbackForce = 12f;
    private float lastAttackInputTime;

    [Header("Skill 1: Mode Panah Settings")]
    public bool isBowMode = false;    
    public GameObject arrowPrefab;     
    public Transform bowFirePoint;     
    public float baseMpCostPerArrow = 1f; // Konsumsi MP dasar
    public LineRenderer aimLine;  
    public float aimDistance = 25f;
    [Range(0f, 1f)] public float aimSpeedMultiplier = 0.2f;
    public float skill1Cooldown = 3f;
    private float lastSkill1Time = -999f;

    [Header("Skill 2: Shield Settings")]
    public bool isShieldActive = false;
    public float maxShieldHp = 0f;
    public float currentShieldHp = 0f;
    public float shieldCooldown = 30f; // BARU: Mengunci cooldown di 30 detik untuk semua level
    public float shieldDuration = 10f;
    public GameObject shieldPrefab;
    private GameObject currentShieldInstance;
    private float lastShieldTime = -999f;
    private bool isCastingShield = false;
    private Coroutine shieldDurationCoroutine;

    [Header("Skill 3: Horizontal Slash Settings")]
    public GameObject slashPrefab;         // Tarik prefab tebasan dari Langkah 1 ke sini
    public Transform slashSpawnPoint;      // Titik muncul tebasan (opsional, bisa diisi posisi koordinat tangan/senjata player)
    public float skill3Cooldown = 5f;     // Jeda waktu penggunaan skill 3
    private float lastSkill3Time = -999f;  
    private bool isCastingSkill3 = false;  // Status penguncian pergerakan saat animasi tebas berjalan

    private Coroutine shieldCoroutine;

    [Header("Prefabs Setting")]
    public GameObject meleeSword;        // Tarik objek Pedang di tangan kanan ke sini
    public GameObject meleeShield;       // Tarik objek Perisai di tangan kiri ke sini
    public GameObject rangeBow;
    public GameObject rangeArrow;

    public float chargeTimer { get; private set; } = 0f;      
    public bool isCharging { get; private set; } = false;     // Status sedang menahan panah

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        speed = PlayerStats.instance.speed;
        attackDamage = PlayerStats.instance.damage;
        currentBlockGauge = maxBlockGauge;

        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.playerBody = this.transform;
        }
    }

    void Update()
    {
        if (damageFlashImage != null && damageFlashImage.color.a > 0)
        {
            damageFlashImage.color = Color.Lerp(damageFlashImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        
        if (PlayerStats.instance != null && PlayerStats.instance.currentHealth <= 0) return; 
        speed = PlayerStats.instance.speed; 
        if (isDashing) return; 

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) 
        {
            float effectiveEnergyCost = isUnlimitedEnergy ? 0f : dashEnergyCost; 

            if (PlayerStats.instance.currentEnergy >= effectiveEnergyCost) 
            {
                // PERBAIKAN: Potong energi player di sini sebelum memicu Dash
                PlayerStats.instance.currentEnergy -= effectiveEnergyCost;

                if (isUsingPotion)
                {
                    CancelPotionAnimation();
                }

                // CANCEL SKILL 1: Mode Panah (Jika sedang menahan busur/charging)
                if (isBowMode && isCharging)
                {
                    isCharging = false;
                    chargeTimer = 0f;
                    if (anim != null) anim.SetBool("isChargingBow", false);
                    if (aimLine != null) aimLine.enabled = false;

                    lastSkill1Time = Time.time + 0.5f - skill1Cooldown; 
                    Debug.Log("Charging Skill 1 dibatalkan! Cooldown diset 3 detik.");
                }

                // CANCEL SKILL 2: Shield (Jika sedang animasi cast sebelum tameng keluar)
                if (isCastingShield)
                {
                    isCastingShield = false;
                    anim.Play("Idle"); 
                    lastShieldTime = Time.time + 3f - shieldCooldown; 
                    Debug.Log("Casting Skill 2 dibatalkan! Cooldown diset 3 detik.");
                }

                // CANCEL SKILL 3: Horizontal Slash (Jika sedang animasi ayunan sebelum proyektil keluar)
                if (isCastingSkill3)
                {
                    isCastingSkill3 = false;
                    anim.Play("Idle"); 
                    lastSkill3Time = Time.time + 3f - skill3Cooldown; 
                    Debug.Log("Casting Skill 3 dibatalkan! Cooldown diset 3 detik.");
                }

                StartCoroutine(DashRoutine());
                return;
            }
        }
        if (isUsingPotion)
        {
            anim.SetFloat("moveX", 0);
            anim.SetFloat("moveZ", 0);
            return; 
        }
        
        LookAtMouse();
        UpdateAnimation();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (Time.time >= lastSkill1Time + skill1Cooldown)
            {
                if (PlayerStats.instance.skill1Level > 0)
                {
                    isBowMode = !isBowMode;   
                    if (anim != null) anim.SetBool("isBowMode", isBowMode);

                    SwitchWeaponModels();
                    ResetCombo();

                    if (!isBowMode && aimLine != null) aimLine.enabled = false;
                    Debug.Log("Mode Panah Jarak Jauh: " + isBowMode);
                    lastSkill1Time = Time.time;
                }
                else
                {
                    Debug.Log("Skill 1 Jarak Jauh Belum Terbuka! Beli di Shop seharga 10 Gold.");
                }
            }
            else
            {
                float sisaCooldown = (lastSkill1Time + skill1Cooldown) - Time.time;
                Debug.Log($"Skill 1 sedang Cooldown! Tunggu {sisaCooldown:F1} detik lagi.");
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isShieldActive || isCastingShield)
            {
                return;
            }

            if (Time.time >= lastShieldTime + shieldCooldown)
            {
                if (PlayerStats.instance.skill2Level > 0)
                {
                    float requiredMp = 10f * PlayerStats.instance.skill2Level;

                    if (PlayerStats.instance.currentMP >= requiredMp)
                    {
                        isCastingShield = true;
                        anim.SetTrigger("ShieldCast"); 
                        lastShieldTime = Time.time; 
                    }
                    else
                    {
                        Debug.Log($"MP Tidak Cukup! Butuh {requiredMp} MP.");
                    }
                }
                else
                {
                    Debug.Log("Skill 2 Shield Belum Terbuka!");
                }
            }
            else
            {
                float sisaCD = (lastShieldTime + shieldCooldown) - Time.time;
                Debug.Log($"Skill 2 sedang Cooldown! Tunggu {sisaCD:F1} detik lagi.");
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            // DI SINI SEKARANG AMAN: isShieldActive dihapus agar tetap bisa menyerang saat tameng aktif
            if (isCastingShield || isCastingSkill3) return;

            if (Time.time >= lastSkill3Time + skill3Cooldown)
            {
                if (PlayerStats.instance.skill3Level <= 0) 
                {
                    Debug.Log("Skill 3 Belum Terbuka!");
                    return;
                }

                float requiredMp = PlayerStats.instance.skill3MpCost;

                if (PlayerStats.instance.currentMP >= requiredMp)
                {
                    isCastingSkill3 = true;
                    anim.SetTrigger("SlashCast"); 
                    lastSkill3Time = Time.time;
                }
                else
                {
                    Debug.Log($"MP Tidak Cukup untuk Skill 3! Butuh {requiredMp} MP (MP Saat Ini: {PlayerStats.instance.currentMP}).");
                }
            }
        }

        if (isBowMode) 
        {
            float currentMpCost = baseMpCostPerArrow * Mathf.Pow(2, PlayerStats.instance.skill1Level - 1); 

            if (Input.GetMouseButton(1))
            {
                isCharging = true;
                chargeTimer += Time.deltaTime;
                if (chargeTimer > 2f) chargeTimer = 2f;
                
                if (anim != null) anim.SetBool("isChargingBow", true);

                if (aimLine != null)
                {
                    aimLine.gameObject.SetActive(true);
                    aimLine.enabled = true;
                    aimLine.SetPosition(0, Vector3.zero); 
                    RaycastHit hit;
                    int layerMaskKecualiPlayer = ~LayerMask.GetMask("Player"); 

                    if (Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, out hit, 30f, layerMaskKecualiPlayer))
                    {
                        aimLine.SetPosition(1, new Vector3(0f, 0f, hit.distance));
                    }
                    else 
                    {
                        aimLine.SetPosition(1, new Vector3(0f, 0f, 30f));
                    }
                }
            }

            if (Input.GetMouseButtonUp(1) && isCharging) 
            {
                isCharging = false; 
                
                if (anim != null) 
                {
                    anim.SetBool("isChargingBow", false); 
                    anim.SetTrigger("releaseShoot");      
                }

                if (aimLine != null) aimLine.enabled = false;
                aimLine.gameObject.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) && !isCharging)
            {
                if (PlayerStats.instance.currentMP >= currentMpCost)
                {
                    if (anim != null) anim.SetTrigger("quickShoot"); 
                }
                else
                {
                    Debug.Log("MP Tidak Cukup untuk Quick Attack!");
                }
            }

            return; 
        }

        bool isCurrentlyAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"); 
        
        if (Input.GetMouseButton(2) && !isDashing && !isCurrentlyAttacking && !isBlockBroken) 
        {
            isBlocking = true; 
            ResetCombo(); 
        }
        else
        {
            isBlocking = false; 
        }

        if (!isBlocking) 
        {
            currentBlockGauge += blockRegenRate * Time.deltaTime; 
            currentBlockGauge = Mathf.Clamp(currentBlockGauge, 0f, maxBlockGauge); 

            if (isBlockBroken && currentBlockGauge >= maxBlockGauge) 
            {
                isBlockBroken = false; 
                Debug.Log("Pertahanan siap digunakan kembali."); 
            }
        }

        anim.SetBool("isBlocking", isBlocking); 

        if (Input.GetKeyDown(KeyCode.Q) && PlayerStats.instance.currentRage >= PlayerStats.instance.maxRage && !PlayerStats.instance.isRageMode) 
        {
            StartCoroutine(ActivateRageMode()); 
        }

        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            if (clickQueueCount < 3)
            {
                clickQueueCount++;
                Debug.Log("Klik masuk antrean! Total saat ini: " + clickQueueCount);
            }

            if (!isCurrentlyAttacking && comboStep == 0)
            {
                comboStep = 1;
                PlayComboAnimation(comboStep);
            }
        }

        if (Input.GetMouseButtonDown(1) && !isBlocking && !isCurrentlyAttacking)
        {
            HeavyAttack();
        }
    }

    void UpdateAnimation()
    {
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        if (isAttacking)
        {
            anim.SetFloat("moveX", 0);
            anim.SetFloat("moveZ", 0);
            return; 
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = new Vector3(x, 0f, z).normalized;
        Vector3 localMove = transform.InverseTransformDirection(moveInput);

        anim.SetFloat("moveX", localMove.x, 0.1f, Time.deltaTime);
        anim.SetFloat("moveZ", localMove.z, 0.1f, Time.deltaTime);
    }

    void LookAtMouse()
    {
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    void FixedUpdate()
    {
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        bool isShootingBow = anim.GetCurrentAnimatorStateInfo(0).IsName("Ranged_Bow_Quick") || 
                             anim.GetCurrentAnimatorStateInfo(0).IsName("Ranged_Bow_Release")||
                             anim.GetCurrentAnimatorStateInfo(0).IsName("Ranged_Bow_Release1");

        if (isAttacking || isDashing || isUsingPotion || isShootingBow)
        {
            if (isUsingPotion)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return;
        }

        if (isCastingShield)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            anim.SetFloat("speed", 0f); 
            return;
        }

        if (isCastingShield || isCastingSkill3)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Kunci gerak horizontal, biarkan gravitasi bekerja
            anim.SetFloat("speed", 0f); 
            return; 
        }
        
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 localMove = transform.InverseTransformDirection(moveInput);
            float currentSpeed = speed * potionSpeedMultiplier * trapSpeedMultiplier;

            if (isBowMode && isCharging)
            {
                currentSpeed *= aimSpeedMultiplier;
            }
            
            if (isBlocking)
            {
                currentSpeed *= blockSpeedMultiplier;
            }

            if (localMove.z <= 0.1f)
            {
                currentSpeed *= 0.7f;
            }

            Vector3 targetPosition = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void PlayComboAnimation(int step)
    {
        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("attack3");

        anim.SetTrigger("attack" + step);
        lastAttackInputTime = Time.time;
    }

    public void CheckComboQueue()
    {
        if (clickQueueCount > comboStep)
        {
            comboStep++; 
            PlayComboAnimation(comboStep); 
        }
        else
        {
            ResetCombo();
        }
    }

    void HeavyAttack()
    {
        anim.SetTrigger("heavyAttack");
        ResetCombo(); 
        Debug.Log("Heavy Attack Dieksekusi!");
    }

    public void ResetCombo()
    {
        clickQueueCount = 0;
        comboStep = 0;
        Debug.Log("Antrean kombo dibersihkan. Kembali ke kondisi normal.");
    }

    public void Hit()
    {
        attackDamage = PlayerStats.instance.damage;
        bool isHeavy = (comboStep == 0);

        float range = isHeavy ? heavyRange : swingRange;
        float finalDamage = isHeavy ? (attackDamage * 2f) : attackDamage;
        finalDamage *= potionDamageMultiplier;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, range, enemyLayers);

        bool isAttack3 = (comboStep == 3);

        foreach (Collider enemy in hitEnemies)
        {
            Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);

            if (isHeavy)
            {
                if (angleToEnemy < 120f) ApplyDamage(enemy, finalDamage, true);
            }
            else
            {
                if (angleToEnemy < swingAngle / 2) ApplyDamage(enemy, finalDamage, isAttack3);
            }
        }
    }

    void ApplyDamage(Collider enemy, float damage, bool causeKnockback)
    {
        Health enemyHealth = enemy.GetComponentInParent<Health>();

        if (impactVFXPrefab != null)
        {
            Vector3 spawnPos = enemy.transform.position + new Vector3(0, 1f, 0);
            GameObject impact = Instantiate(impactVFXPrefab, spawnPos, Quaternion.identity);
            
            if (PlayerStats.instance != null && PlayerStats.instance.isRageMode)
            {
                ParticleSystem[] allParticles = impact.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in allParticles)
                {
                    var main = ps.main;
                    main.startColor = Color.red; 
                }
            }
            Destroy(impact, 0.8f);
        }

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            if (PlayerStats.instance.isRageMode)
            {
                float healAmount = damage; 
                PlayerStats.instance.currentHealth = Mathf.Clamp(PlayerStats.instance.currentHealth + healAmount, 0, PlayerStats.instance.maxHealth);
            }
        }

        // Perhatikan penulisan nama Layer di bawah ini, sesuaikan huruf kapitalnya dengan di Unity Editor kamu ("HeavyEnemy" / "Boss")
        int heavyEnemyLayer = LayerMask.NameToLayer("HeavyEnemy");
        int bossLayer = LayerMask.NameToLayer("Boss");

        if (causeKnockback && enemy.gameObject.layer != bossLayer && enemy.gameObject.layer != heavyEnemyLayer)
        {
            // --- PERBAIKAN 2: Gunakan GetComponentInParent juga untuk mengambil script AI-nya ---
            EnemyAI enemyAI = enemy.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                knockbackDirection.y = 0; 

                enemyAI.TakeKnockback(knockbackDirection, knockbackForce);
            }
        }
    }

    public void OnPlayerHit()
    {
        isUsingPotion = false; 
        ResetCombo();          
        Debug.Log("Player terluka: Status Minum & Antrian Kombo berhasil di-reset!");

        if (damageFlashImage != null)
        {
            damageFlashImage.color = flashColor;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, swingRange);
        }
    }

    public void TriggerNormalSlash()
    {
        GameObject selectedPrefab = null;
        Transform selectedSpawnPoint = null;

        switch (comboStep)
        {
            case 1:
                selectedPrefab = slashPrefabCombo1;
                selectedSpawnPoint = spawnPointCombo1;
                break;
            case 2:
                selectedPrefab = slashPrefabCombo2;
                selectedSpawnPoint = spawnPointCombo2;
                break;
            case 3:
                selectedPrefab = slashPrefabCombo3;
                selectedSpawnPoint = spawnPointCombo3;
                break;
            default:
                selectedPrefab = slashPrefabCombo1;
                selectedSpawnPoint = spawnPointCombo1;
                break;
        }

        if (selectedPrefab != null && selectedSpawnPoint != null)
        {
            SpawnVFX(selectedPrefab, selectedSpawnPoint);
        }
    }

    public void TriggerHeavySlash()
    {
        SpawnVFX(heavySlashPrefab, heavySpawnPoint);
    }

    private void SpawnVFX(GameObject prefab, Transform spawnPoint)
    {
        if (prefab != null && spawnPoint != null)
        {
            GameObject vfx = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            
            if (PlayerStats.instance != null && PlayerStats.instance.isRageMode)
            {
                var ps = vfx.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startColor = Color.red;
                }
            }
            Destroy(vfx, 1.0f);
        }
    }

    public bool AbsorbDamageWithBlock(float amount)
    {
        if (!isBlocking || isBlockBroken) return false; 

        currentBlockGauge -= amount;
        Debug.Log("Serangan ditahan! Sisa kekuatan block: " + currentBlockGauge);

        if (currentBlockGauge <= 0)
        {
            currentBlockGauge = 0;
            isBlocking = false;
            isBlockBroken = true;
            anim.SetTrigger("blockBreak"); 
            Debug.Log("BLOCK BREAK! Pertahanan hancur total.");
        }

        return true; 
    }

    public void TriggerPotionAnimation(int type)
    {
        if (isBowMode)
        {
            Debug.Log("Tidak bisa menggunakan item ramuan saat sedang dalam Mode Panah!");
            return; 
        }
        
        if (PlayerStats.instance.currentHealth <= 0 || isDashing || isUsingPotion) 
            return;

        if (type == 1 && (PlayerStats.instance.smallPotionCount <= 0 || PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth)) return;
        if (type == 2 && (PlayerStats.instance.mediumPotionCount <= 0 || PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth)) return;
        if (type == 3 && (PlayerStats.instance.largePotionCount <= 0 || PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth)) return;
        if (type == 4 && (PlayerStats.instance.smallMPCount <= 0 || PlayerStats.instance.currentMP >= PlayerStats.instance.maxMP)) return;
        if (type == 5 && (PlayerStats.instance.mediumMPCount <= 0 || PlayerStats.instance.currentMP >= PlayerStats.instance.maxMP)) return;
        if (type == 6 && (PlayerStats.instance.largeMPCount <= 0 || PlayerStats.instance.currentMP >= PlayerStats.instance.maxMP)) return;
        if (type == 7 && PlayerStats.instance.energyPotionCount <= 0) return;
        if (type == 8 && PlayerStats.instance.strengthPotionCount <= 0) return;
        if (type == 9 && PlayerStats.instance.speedPotionCount <= 0) return;

        isUsingPotion = true;
        ResetCombo(); 

        anim.SetInteger("potionType", type);
        anim.SetTrigger("usePotion");
    }
    
    public void ExecutePotionEffect()
    {
        if (!isUsingPotion) return; 

        int type = anim.GetInteger("potionType");

        switch (type)
        {
            case 1: 
                PlayerStats.instance.smallPotionCount--;
                PlayerStats.instance.currentHealth += PlayerStats.instance.smallHealAmount;
                break;
            case 2: 
                PlayerStats.instance.mediumPotionCount--;
                PlayerStats.instance.currentHealth += PlayerStats.instance.mediumHealAmount;
                break;
            case 3: 
                PlayerStats.instance.largePotionCount--;
                PlayerStats.instance.currentHealth += PlayerStats.instance.largeHealAmount;
                break;
            case 4: 
                PlayerStats.instance.smallMPCount--;
                PlayerStats.instance.currentMP += PlayerStats.instance.smallMPAmount;
                PlayerStats.instance.currentMP = Mathf.Clamp(PlayerStats.instance.currentMP, 0f, PlayerStats.instance.maxMP);
                break;
             case 5: 
                PlayerStats.instance.mediumMPCount--;
                PlayerStats.instance.currentMP += PlayerStats.instance.mediumMPAmount;
                PlayerStats.instance.currentMP = Mathf.Clamp(PlayerStats.instance.currentMP, 0f, PlayerStats.instance.maxMP);
                break;
             case 6: 
                PlayerStats.instance.largeMPCount--;
                PlayerStats.instance.currentMP += PlayerStats.instance.largeMPAmount;
                PlayerStats.instance.currentMP = Mathf.Clamp(PlayerStats.instance.currentMP, 0f, PlayerStats.instance.maxMP);
                break;
            case 7: 
                PlayerStats.instance.energyPotionCount--;
                StartCoroutine(EnergyBuffRoutine());
                break;
            case 8: 
                PlayerStats.instance.strengthPotionCount--;
                StartCoroutine(StrengthBuffRoutine());
                break;
            case 9: 
                PlayerStats.instance.speedPotionCount--;
                StartCoroutine(SpeedBuffRoutine());
                break;
        }

        PlayerStats.instance.currentHealth = Mathf.Clamp(PlayerStats.instance.currentHealth, 0f, PlayerStats.instance.maxHealth);
    }

    private void CancelPotionAnimation()
    {
        isUsingPotion = false;
        anim.SetInteger("potionType", 0);
        anim.Play("Movement"); 
        Debug.Log("Animasi minum potion DIBATALKAN karena Player melakukan Dash!");
    }

    public void FinishPotionAnimation()
    {
        if (!isUsingPotion) return;

        isUsingPotion = false;
        anim.SetInteger("potionType", 0);
        Debug.Log("Animasi Potion Selesai Penuh. Karakter bebas bergerak kembali.");
    }

    public void CastSkill(int skillID)
    {
        if (PlayerStats.instance.currentHealth <= 0 || isDashing || isUsingPotion) return;

        if (skillID == 1)
        {
            if (PlayerStats.instance.skill1Level > 0 && PlayerStats.instance.currentMP >= PlayerStats.instance.skill1MpCost)
            {
                PlayerStats.instance.currentMP -= PlayerStats.instance.skill1MpCost;
                Debug.Log("MENGGUNAKAN SKILL 1 (FIREBALL)! Damage: " + (PlayerStats.instance.damage * 2f * PlayerStats.instance.skill1Level));
            }
            else
            {
                Debug.Log("Skill 1 terkunci atau MP tidak cukup!");
            }
        }
        else if (skillID == 2)
        {
            if (isShieldActive || isCastingShield) return;

            if (PlayerStats.instance.skill2Level > 0)
            {
                float requiredMp = 10f * PlayerStats.instance.skill2Level;

                if (PlayerStats.instance.currentMP >= requiredMp)
                {
                    isCastingShield = true;
                    anim.SetTrigger("ShieldCast");
                    lastShieldTime = Time.time;
                }
                else
                {
                    Debug.Log($"MP Tidak Cukup untuk Skill 2! Butuh {requiredMp} MP.");
                }
            }
            else
            {
                Debug.Log("Skill 2 terkunci!");
            }
        }
        else if (skillID == 3)
        {
            // PERBAIKAN: Hapus 'isShieldActive' dari baris ini juga
            if (isCastingShield || isCastingSkill3) return; 

            float requiredMp = 25f;
            if (PlayerStats.instance.currentMP >= requiredMp)
            {
                isCastingSkill3 = true;
                anim.SetTrigger("SlashCast");
                lastSkill3Time = Time.time;
            }
            else
            {
                Debug.Log("MP Tidak Cukup untuk Skill 3!");
            }
        }
    }

    private void SwitchWeaponModels()
    {
        if (isBowMode)
        {
            if (meleeSword != null) meleeSword.SetActive(false);
            if (meleeShield != null) meleeShield.SetActive(false);
            if (rangeBow != null) rangeBow.SetActive(true);
        }
        else
        {
            if (meleeSword != null) meleeSword.SetActive(true);
            if (meleeShield != null) meleeShield.SetActive(true);
            if (rangeBow != null) rangeBow.SetActive(false);
        }
    }

    public void TriggerQuickShootEvent()
    {
        float currentMpCost = baseMpCostPerArrow * Mathf.Pow(2, PlayerStats.instance.skill1Level - 1);
        float currentDamage = PlayerStats.instance.damage * PlayerStats.instance.skill1Level;

        if (PlayerStats.instance.currentMP >= currentMpCost)
        {
            ShootArrow(currentDamage, false);
            PlayerStats.instance.currentMP -= currentMpCost;
        }
    }

    public void TriggerChargeShootEvent()
    {
        float currentMpCost = baseMpCostPerArrow * Mathf.Pow(2, PlayerStats.instance.skill1Level - 1);
        float currentDamage = PlayerStats.instance.damage * PlayerStats.instance.skill1Level;

        if (PlayerStats.instance.currentMP >= currentMpCost)
        {
            float damageMultiplier = 1f + (chargeTimer / 2f) * 2f; 
            float finalDamage = currentDamage * damageMultiplier;

            ShootArrow(finalDamage, true);
            PlayerStats.instance.currentMP -= currentMpCost;
        }
        else
        {
            Debug.Log("MP Tidak Cukup untuk Charge Attack!");
        }

        chargeTimer = 0f; 
    }

    void ShootArrow(float damageAmount, bool isChargedAttack)
    {
        if (arrowPrefab != null && bowFirePoint != null)
        {
            Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayLength;
            Quaternion rotasiPanah = bowFirePoint.rotation; 

            if (groundPlane.Raycast(cameraRay, out rayLength))
            {
                Vector3 titikMouse = cameraRay.GetPoint(rayLength);
                Vector3 targetSejajar = new Vector3(titikMouse.x, bowFirePoint.position.y, titikMouse.z);
                Vector3 arahTerbang = (targetSejajar - bowFirePoint.position).normalized;
                
                rotasiPanah = Quaternion.LookRotation(arahTerbang);
            }

            GameObject arrowObj = Instantiate(arrowPrefab, bowFirePoint.position, rotasiPanah);
            ArrowProjectile arrowScript = arrowObj.GetComponent<ArrowProjectile>();
            
            if (arrowScript != null)
            {
                arrowScript.SetupProjectile(damageAmount, isChargedAttack);
            }
        }
    }

  private IEnumerator ActivateShieldRoutine()
    {
        float shieldPercent = 0.25f * PlayerStats.instance.skill2Level;
        maxShieldHp = PlayerStats.instance.maxHealth * shieldPercent; 
        currentShieldHp = maxShieldHp;

        float dynamicDuration = 10f * PlayerStats.instance.skill2Level;
        isShieldActive = true;

        if (shieldPrefab != null) 
        {
            currentShieldInstance = Instantiate(shieldPrefab, transform.position, transform.rotation, transform); 
        }

        // Tunggu sampai durasi waktu habis
        yield return new WaitForSeconds(dynamicDuration);

        // Jika waktu habis dan shield belum hancur oleh musuh, panggil fungsi hancur
        BreakShield();
        Debug.Log("Shield Berakhir karena durasi habis!");
    }

    public void BreakShield()
    {
        // Matikan status aktif shield
        isShieldActive = false;
        currentShieldHp = 0f; 
        maxShieldHp = 0f;

        // LANGSUNG HANCURKAN PREFAB SHIELD JIKA MASIH ADA
        if (currentShieldInstance != null) 
        {
            Destroy(currentShieldInstance);
        }

        // Hentikan coroutine waktu agar tidak berjalan di latar belakang jika hancur duluan oleh damage
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
    }

    public void AnimationEvent_TriggerShield()
    {
        // JIKA player menekan Dash sebelum event ini terpicu, batalkan pembuatan tameng!
        if (!isCastingShield) return;

        // Buka kembali kunci pergerakan karena animasi pelepasan skill selesai
        isCastingShield = false; 

        float requiredMp = 10f * PlayerStats.instance.skill2Level;
        if (PlayerStats.instance.currentMP >= requiredMp)
        {
            // MP baru resmi dipotong di sini setelah dipastikan tidak dicancel oleh dash
            PlayerStats.instance.currentMP -= requiredMp;

            // Kalkulasi ketahanan tameng
            float shieldPercent = 0.25f * PlayerStats.instance.skill2Level;
            maxShieldHp = PlayerStats.instance.maxHealth * shieldPercent; 
            currentShieldHp = maxShieldHp;

            isShieldActive = true;
            Debug.Log($"Shield aktif melalui Animation Event! HP Tameng: {currentShieldHp}");

            // Munculkan Prefab Tameng
            if (shieldPrefab != null)
            {
                currentShieldInstance = Instantiate(shieldPrefab, transform.position, transform.rotation, transform);
            }

            // Jalankan hitung mundur durasi aktif tameng
            float dynamicDuration = 10f * PlayerStats.instance.skill2Level;
            if (shieldDurationCoroutine != null) StopCoroutine(shieldDurationCoroutine);
            shieldDurationCoroutine = StartCoroutine(ShieldDurationCountdown(dynamicDuration));
        }
    }

    private IEnumerator ShieldDurationCountdown(float duration)
    {
        yield return new WaitForSeconds(duration);
        BreakShield();
        Debug.Log("Shield Berakhir karena durasi waktu habis!");
    }

    public void AnimationEvent_TriggerHorizontalSlash()
    {
        if (!isCastingSkill3) return;
        isCastingSkill3 = false;

        // PERBAIKAN 1: Ambil MP Cost dinamis berdasarkan level dari PlayerStats (20, 40, 60, dst)
        float requiredMp = PlayerStats.instance.skill3MpCost; 

        if (PlayerStats.instance.currentMP >= requiredMp)
        {
            PlayerStats.instance.currentMP -= requiredMp;

            Vector3 spawnPos = slashSpawnPoint != null ? slashSpawnPoint.position : transform.position + transform.forward;
            Quaternion spawnRot = transform.rotation; 

            if (slashPrefab != null)
            {
                // 1. Munculkan prefab tebasan ke scene game
                GameObject slashObj = Instantiate(slashPrefab, spawnPos, spawnRot);
                
                // PERBAIKAN 2: Ubah ukuran skala proyektil tebasan berdasarkan level (Lvl 1 = 1x, Lvl 2 = 2x, Lvl 3 = 3x, dst)
                float currentScale = PlayerStats.instance.skill3ScaleMultiplier;
                slashObj.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                
                // 2. Ambil komponen HorizontalSlash dari prefab yang baru muncul
                HorizontalSlash slashScript = slashObj.GetComponent<HorizontalSlash>();
                
                if (slashScript != null)
                {
                    // PERBAIKAN 3: Hitung damage dinamis berdasarkan level dari PlayerStats (Lvl 1 = 200%, Lvl 2 = 400%, dst)
                    float totalSkillDamage = PlayerStats.instance.damage * PlayerStats.instance.skill3DamageMultiplier; 
                    
                    // 3. Kirim nilai damage ke fungsi SetupSlash
                    slashScript.SetupSlash(totalSkillDamage);
                }

                Debug.Log($"Skill 3 Level {PlayerStats.instance.skill3Level} Berhasil Diluncurkan! Damage: {PlayerStats.instance.damage * PlayerStats.instance.skill3DamageMultiplier}");
            }
            else
            {
                Debug.LogError("Prefab Slash Belum Dimasukkan ke Inspector PlayerMovement!");
            }
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        isImmune = true; 
        lastDashTime = Time.time;
        ResetCombo();

        if (isBowMode)
        {
            isCharging = false;   
            chargeTimer = 0f;     
            
            if (anim != null) 
            {
                anim.SetBool("isChargingBow", false); 
            }

            if (aimLine != null) 
            {
                aimLine.enabled = false;             
                aimLine.gameObject.SetActive(false); 
            }
        }

        anim.SetFloat("moveX", 0);
        anim.SetFloat("moveZ", 0);
        anim.SetBool("isWalking", false);

        Vector3 dashDirection = transform.forward; 
        anim.SetTrigger("dash");

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }

        isDashing = false;
        rb.linearVelocity = Vector3.zero;

        float extraImmuneTime = invincibilityDuration - dashDuration;
        if (extraImmuneTime > 0)
        {
            yield return new WaitForSeconds(extraImmuneTime);
        }

        isImmune = false;
    }

    IEnumerator ActivateRageMode()
    {
        PlayerStats.instance.isRageMode = true;
        PlayerStats.instance.currentRage = 0;

        float originalSpeed = speed;
        speed *= 1.5f;
        anim.speed = rageAnimSpeed;

        if (rageAuraPrefab != null && activeAura == null)
        {
            activeAura = Instantiate(rageAuraPrefab, transform.position, transform.rotation, transform);
            activeAura.transform.localPosition = new Vector3(0, 0.2f, 0);
        }

        Debug.Log("RAGE MODE AKTIF!");

        float duration = 10f;
        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            PlayerStats.instance.currentRage = timer * 10f; 
            yield return null;
        }

        PlayerStats.instance.currentRage = 0;

        if (activeAura != null)
        {
            ParticleSystem[] particles = activeAura.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                var emission = ps.emission;
                emission.enabled = false; 
            }
            
            Destroy(activeAura, 1f);
            activeAura = null; 
        }

        PlayerStats.instance.isRageMode = false;
        speed = originalSpeed;
        anim.speed = 1f;

        PlayerStats.instance.currentMP = 0;
        PlayerStats.instance.currentEnergy = 0;
        
        Debug.Log("Rage berakhir. Player kelelahan!");
    }

    IEnumerator EnergyBuffRoutine()
    {
        isUnlimitedEnergy = true;
        Debug.Log("Buff Terpasang: Unlimited Energy selama 10 Detik!");
        yield return new WaitForSeconds(10f);
        isUnlimitedEnergy = false;
        Debug.Log("Buff Berakhir: Unlimited Energy Selesai.");
    }

    IEnumerator StrengthBuffRoutine()
    {
        potionDamageMultiplier = 2f;
        Debug.Log("Buff Terpasang: Damage x2 selama 10 Detik!");
        yield return new WaitForSeconds(10f);
        potionDamageMultiplier = 1f;
        Debug.Log("Buff Berakhir: Kekuatan kembali normal.");
    }

    IEnumerator SpeedBuffRoutine()
    {
        potionSpeedMultiplier = 2f;
        Debug.Log("Buff Terpasang: Kecepatan x2 selama 10 Detik!");
        yield return new WaitForSeconds(10f);
        potionSpeedMultiplier = 1f;
        Debug.Log("Buff Berakhir: Kecepatan kembali normal.");
    }

    public void ApplyFreeze(float duration)
    {
        // Jalankan Coroutine freeze tanpa menumpuk jika sedang melambat
        StopCoroutine("FreezeRoutine"); 
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        // 1. Set multiplier menjadi 0.4f (Kecepatan berkurang 60%, sisa 40%)
        trapSpeedMultiplier = 0.1f; 

        // 2. Tunggu selama durasi freeze yang ditentukan oleh boss
        yield return new WaitForSeconds(duration);

        // 3. Kembalikan multiplier ke normal (1.0f = 100% speed)
        trapSpeedMultiplier = 1f;
    }
}