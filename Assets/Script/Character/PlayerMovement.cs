using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody rb;
    private Camera mainCamera;
    private Animator anim; // Tambahkan ini
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
    public float blockSpeedMultiplier = 0f;   // Kecepatan gerak saat blok (40% dari speed normal)
    public bool isBlocking { get; private set; }
    public bool isBlockBroken { get; private set; } = false;
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 3f;
    public float invincibilityDuration = 0.5f; // Durasi immune yang bisa kamu atur
    public bool isImmune { get; private set; } // Variabel baru untuk status immune
    public bool isDashing { get; private set; }
    private float lastDashTime;

    [Header("Skill Costs")]
    public float dashMPCost = 10f;
    public float heavyAttackMPCost = 15f;

    [Header("Energy Costs")]
    public float dashEnergyCost = 5f;

    [Header("Potion Buff Status")]
    private bool isUnlimitedEnergy = false;
    private float potionDamageMultiplier = 1f;
    private float potionSpeedMultiplier = 1f;

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
    public int comboStep = 0;          // Mencatat tahapan animasi yang sedang aktif (0-3)
    public int clickQueueCount = 0;    // Mencatat jumlah klik yang berhasil ditampung (Maksimal 3)
    public float knockbackForce = 12f;
    private float lastAttackInputTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;

        // TAMBAHKAN INI: Mengunci rotasi X dan Z agar karakter tidak terguling
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Ambil speed dan damage dari stats
        speed = PlayerStats.instance.speed;
        attackDamage = PlayerStats.instance.damage;
        currentBlockGauge = maxBlockGauge;

        // Daftarkan transform ksatria ini ke PlayerStats
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.playerBody = this.transform;
        }
    }

    void Update()
    {
        if (PlayerStats.instance != null && PlayerStats.instance.currentHealth <= 0) return;
        speed = PlayerStats.instance.speed;
        if (isDashing) return;

        // 1. INPUT DASH (Ditaruh di paling atas agar bisa membatalkan animasi potion)
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space))
        {
            float effectiveEnergyCost = isUnlimitedEnergy ? 0f : dashEnergyCost;

            if (PlayerStats.instance.currentEnergy >= dashEnergyCost)
            {
                // JIKA SEDANG MINUM POTION, BATALKAN TERLEBIH DAHULU
                if (isUsingPotion)
                {
                    CancelPotionAnimation();
                }

                if (!isUnlimitedEnergy)
                {
                    PlayerStats.instance.currentEnergy -= dashEnergyCost;
                }

                //PlayerStats.instance.currentEnergy -= dashEnergyCost;
                StartCoroutine(DashRoutine());
                return; // Keluar dari update karena sudah masuk state dash
            }
        }

        // 2. LOCK INPUT JIKA SEDANG MINUM POTION
        // Semua input di bawah baris ini (jalan, menyerang, block) akan diabaikan
        if (isUsingPotion)
        {
            anim.SetFloat("moveX", 0);
            anim.SetFloat("moveZ", 0);
            return; 
        }
        
        LookAtMouse();
        UpdateAnimation();

        // --- INPUT BLOCK (KLIK TENGAH MOUSE / BUTTON 2) ---
        bool isCurrentlyAttacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        
        // Player bisa blok jika menekan klik tengah, tidak sedang dash/attack, dan tameng tidak sedang hancur
        if (Input.GetMouseButton(2) && !isDashing && !isCurrentlyAttacking && !isBlockBroken)
        {
            isBlocking = true;
            ResetCombo(); // Batalkan antrean kombo saat mendadak bertahan
        }
        else
        {
            isBlocking = false;
        }

        // --- LOGIKA PEMULIHAN BLOCK GAUGE ---
        if (!isBlocking)
        {
            currentBlockGauge += blockRegenRate * Time.deltaTime;
            currentBlockGauge = Mathf.Clamp(currentBlockGauge, 0f, maxBlockGauge);

            // Jika block sempat hancur, beri syarat harus pulih minimal 20% baru bisa dipake lagi
            if (isBlockBroken && currentBlockGauge >= maxBlockGauge)
            {
                isBlockBroken = false;
                Debug.Log("Pertahanan siap digunakan kembali.");
            }
        }

        // Sinkronisasi parameter ke Animator (Buat parameter Bool bernama "isBlocking" di Unity Animator)
        anim.SetBool("isBlocking", isBlocking);

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space))
        {
            if (PlayerStats.instance.currentEnergy >= dashEnergyCost)
            {
                PlayerStats.instance.currentEnergy -= dashEnergyCost;
                StartCoroutine(DashRoutine());
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && PlayerStats.instance.currentRage >= PlayerStats.instance.maxRage && !PlayerStats.instance.isRageMode)
        {
            StartCoroutine(ActivateRageMode());
        }

        // --- MODIFIKASI: Hanya bisa menyerang jika TIDAK sedang memblokir ---
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

        if (isAttacking || isDashing || isUsingPotion)
        {
            if (isUsingPotion)
            {
                // Hentikan kecepatan sumbu X dan Z agar tidak bergeser saat minum
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            return;
        }
        
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 localMove = transform.InverseTransformDirection(moveInput);
            float currentSpeed = speed * potionSpeedMultiplier;;

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
        // 1. Cek apakah animasi yang sedang jalan adalah Heavy Attack atau Attack biasa
        //bool isHeavy = anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack");
        bool isHeavy = (comboStep == 0);

        // 2. Tentukan range dan damage berdasarkan jenis animasi yang sedang aktif
        float range = isHeavy ? heavyRange : swingRange;
        float finalDamage = isHeavy ? (attackDamage * 2f) : attackDamage;
        finalDamage *= potionDamageMultiplier;

        // 3. Deteksi musuh dalam radius attackPoint
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
        Health enemyHealth = enemy.GetComponent<Health>();

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
                float healAmount = damage; // 10% lifesteal
                PlayerStats.instance.currentHealth = Mathf.Clamp(PlayerStats.instance.currentHealth + healAmount, 0, PlayerStats.instance.maxHealth);
            }
        }

        if (causeKnockback)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                // Hitung arah dorongan (dari posisi player ke posisi musuh)
                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                knockbackDirection.y = 0; // Kunci sumbu Y agar musuh tidak melayang ke langit

                enemyAI.TakeKnockback(knockbackDirection, knockbackForce);
            }
        }
    }

    public void OnPlayerHit()
    {
        isUsingPotion = false; // 1. Reset status minum ramuan agar kontrol tidak membeku
        ResetCombo();          // 2. Bersihkan antrian kombo yang menumpuk
        
        // Opsional: Matikan status dash jika tidak ingin player lanjut meluncur saat terpukul
        // isDashing = false; 
        
        Debug.Log("Player terluka: Status Minum & Antrian Kombo berhasil di-reset!");
    }

    // Untuk melihat jangkauan serangan di Scene
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

    // Fungsi internal agar kode tidak duplikat
    private void SpawnVFX(GameObject prefab, Transform spawnPoint)
    {
        if (prefab != null && spawnPoint != null)
        {
            GameObject vfx = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            
            // Tambahkan logika warna merah di sini
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
        if (!isBlocking || isBlockBroken) return false; // Gagal memblokir jika sedang tidak pasang kuda-kuda

        currentBlockGauge -= amount;
        Debug.Log("Serangan ditahan! Sisa kekuatan block: " + currentBlockGauge);

        if (currentBlockGauge <= 0)
        {
            currentBlockGauge = 0;
            isBlocking = false;
            isBlockBroken = true;
            anim.SetTrigger("blockBreak"); // Jalankan trigger animasi pertahanan jebol jika ada
            Debug.Log("BLOCK BREAK! Pertahanan hancur total.");
        }

        return true; // Sukses memblokir damage sepenuhnya
    }

    public void TriggerPotionAnimation(int type)
    {
        if (PlayerStats.instance.currentHealth <= 0 || isDashing || isUsingPotion) 
            return;

        // Validasi ketersediaan barang & kondisi penuh
        if (type == 1 && (PlayerStats.instance.smallPotionCount <= 0 || PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth)) return;
        if (type == 2 && (PlayerStats.instance.mediumPotionCount <= 0 || PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth)) return;
        if (type == 3 && (PlayerStats.instance.largePotionCount <= 0 || PlayerStats.instance.currentHealth >= PlayerStats.instance.maxHealth)) return;
        if (type == 4 && (PlayerStats.instance.smallMPCount <= 0 || PlayerStats.instance.currentMP >= PlayerStats.instance.maxMP)) return;
        if (type == 5 && PlayerStats.instance.energyPotionCount <= 0) return;
        if (type == 6 && PlayerStats.instance.strengthPotionCount <= 0) return;
        if (type == 7 && PlayerStats.instance.speedPotionCount <= 0) return;

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
            case 1: // Small HP
                PlayerStats.instance.smallPotionCount--;
                PlayerStats.instance.currentHealth += PlayerStats.instance.smallHealAmount;
                break;
            case 2: // Medium HP
                PlayerStats.instance.mediumPotionCount--;
                PlayerStats.instance.currentHealth += PlayerStats.instance.mediumHealAmount;
                break;
            case 3: // Large HP
                PlayerStats.instance.largePotionCount--;
                PlayerStats.instance.currentHealth += PlayerStats.instance.largeHealAmount;
                break;
            case 4: // MP Potion
                PlayerStats.instance.smallMPCount--;
                PlayerStats.instance.currentMP += PlayerStats.instance.smallMPAmount;
                PlayerStats.instance.currentMP = Mathf.Clamp(PlayerStats.instance.currentMP, 0f, PlayerStats.instance.maxMP);
                Debug.Log("MP Berhasil Dipulihkan!");
                break;
            case 5: // Energy Potion (Unlimited 10s)
                PlayerStats.instance.energyPotionCount--;
                StartCoroutine(EnergyBuffRoutine());
                break;
            case 6: // Strength Potion (Damage x2 10s)
                PlayerStats.instance.strengthPotionCount--;
                StartCoroutine(StrengthBuffRoutine());
                break;
            case 7: // Speed Potion (Speed x2 10s)
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
        
        // Paksa animator memotong animasi minum dan kembali ke Blend Tree bergerak/idle
        // Sesuaikan "Movement" dengan nama State dasar pergerakan di Animator kamu
        anim.Play("Movement"); 

        Debug.Log("Animasi minum potion DIBATALKAN karena Player melakukan Dash!");
    }

    public void FinishPotionAnimation()
    {
        if (!isUsingPotion) return;

        // Kunci baru dibuka di sini setelah seluruh gerakan selesai
        isUsingPotion = false;
        anim.SetInteger("potionType", 0);
        Debug.Log("Animasi Potion Selesai Penuh. Karakter bebas bergerak kembali.");
    }

    // --- FITUR BARU: EKSEKUSI SKILL ---
    public void CastSkill(int skillID)
    {
        // Pengecekan dasar: jika player mati atau sedang dash, gagalkan skill
        if (PlayerStats.instance.currentHealth <= 0 || isDashing || isUsingPotion) return;

        if (skillID == 1)
        {
            // Cek apakah skill sudah dibuka dan MP cukup
            if (PlayerStats.instance.skill1Level > 0 && PlayerStats.instance.currentMP >= PlayerStats.instance.skill1MpCost)
            {
                PlayerStats.instance.currentMP -= PlayerStats.instance.skill1MpCost;
                
                // LOGIKA SKILL 1: Serangan Fireball (Contoh efek)
                Debug.Log("MENGGUNAKAN SKILL 1 (FIREBALL)! Damage: " + (PlayerStats.instance.damage * 2f * PlayerStats.instance.skill1Level));
                
                // Kamu bisa memicu trigger animasi skill di sini jika ada, contoh:
                // anim.SetTrigger("skill1");
            }
            else
            {
                Debug.Log("Skill 1 terkunci atau MP tidak cukup!");
            }
        }
        else if (skillID == 2)
        {
            if (PlayerStats.instance.skill2Level > 0 && PlayerStats.instance.currentMP >= PlayerStats.instance.skill2MpCost)
            {
                PlayerStats.instance.currentMP -= PlayerStats.instance.skill2MpCost;

                // LOGIKA SKILL 2: Heal Kecil instan memanfaatkan Level Skill
                float healAmount = 2f * PlayerStats.instance.skill2Level;
                PlayerStats.instance.currentHealth += healAmount;
                PlayerStats.instance.currentHealth = Mathf.Clamp(PlayerStats.instance.currentHealth, 0f, PlayerStats.instance.maxHealth);

                Debug.Log("MENGGUNAKAN SKILL 2 (HEAL BUFF)!");
            }
            else
            {
                Debug.Log("Skill 2 terkunci atau MP tidak cukup!");
            }
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        isImmune = true; // Mulai masa immune
        lastDashTime = Time.time;
        ResetCombo();

        anim.SetFloat("moveX", 0);
        anim.SetFloat("moveZ", 0);
        anim.SetBool("isWalking", false);

        Vector3 dashDirection = transform.forward; 
        anim.SetTrigger("dash");

        // 2. Eksekusi Dash
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }

        isDashing = false;
        rb.linearVelocity = Vector3.zero;

        //TAMBAHKAN INI: Aktifkan kembali tabrakan setelah meluncur
        //if (myCollider != null) myCollider.isTrigger = false;

        // 3. Masa Immune bisa lebih lama dari durasi gerak dash itu sendiri
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

        // 1. Bonus Atribut: Speed & Animasi
        float originalSpeed = speed;
        speed *= 1.5f;
        anim.speed = rageAnimSpeed;

        if (rageAuraPrefab != null && activeAura == null)
        {
            // Munculkan sebagai child dari Player (transform) agar aura mengikuti gerakan player
            activeAura = Instantiate(rageAuraPrefab, transform.position, transform.rotation, transform);
            
            // Sedikit offset ke atas agar posisi aura pas di badan, bukan terkubur di lantai
            activeAura.transform.localPosition = new Vector3(0, 0.2f, 0);
        }

        Debug.Log("RAGE MODE AKTIF!");

        // 2. Durasi Rage (100 detik)
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
                emission.enabled = false; // Hentikan partikel baru agar memudar halus
            }
            
            // Hancurkan objek aura sepenuhnya setelah 1 detik (menunggu sisa partikel hilang)
            Destroy(activeAura, 1f);
            activeAura = null; // Kosongkan referensi
        }

        // 3. Rage Selesai: Pinalti Kelelahan
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
}