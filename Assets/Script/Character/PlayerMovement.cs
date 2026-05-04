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

    public Transform attackPoint; // Titik di depan pedang
    public LayerMask enemyLayers; // Pilih layer "Enemy" di Inspector

    [Header("Rage Mode Settings")]
    public float rageDuration = 100f;
    public float rageAnimSpeed = 1.5f; // Mempercepat animasi 1.5x

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // Ambil komponen Animator
        mainCamera = Camera.main;

        // TAMBAHKAN INI: Mengunci rotasi X dan Z agar karakter tidak terguling
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Ambil speed dan damage dari stats
        speed = PlayerStats.instance.speed;
        attackDamage = PlayerStats.instance.damage;

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
        LookAtMouse();
        UpdateAnimation();

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space))
        {
            if (PlayerStats.instance.currentEnergy >= dashEnergyCost)
            {
                PlayerStats.instance.currentEnergy -= dashEnergyCost; // Kurangi Energy
                StartCoroutine(DashRoutine());
            }
            else
            {
                Debug.Log("Energy tidak cukup untuk Dash!");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && PlayerStats.instance.currentRage >= PlayerStats.instance.maxRage && !PlayerStats.instance.isRageMode)
            {
                StartCoroutine(ActivateRageMode());
            }

        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) return;
        if (Input.GetMouseButtonDown(0)) Attack(false);
        if (Input.GetMouseButtonDown(1)) Attack(true);
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

        // Sangat Penting: Ubah arah gerak dunia menjadi arah lokal karakter
        // Agar jika kita jalan mundur sambil melihat mouse, animasinya pun mundur
        Vector3 localMove = transform.InverseTransformDirection(moveInput);

        // Kirim nilai ke Parameter Animator (moveX dan moveZ harus sama persis namanya)
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

        if (isAttacking || isDashing)
        {
            // Hentikan semua kecepatan gerak saat menyerang
            //rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 targetPosition = rb.position + moveInput * speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
        else
        {
            // Pastikan kecepatan linear di-reset saat tidak ada input agar tidak "meluncur"
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void Attack(bool isHeavy)
    {
        string triggerName = isHeavy ? "heavyAttack" : "attack";
        anim.SetTrigger(triggerName);
    }

    public void Hit()
    {
        attackDamage = PlayerStats.instance.damage;
        // 1. Cek apakah animasi yang sedang jalan adalah Heavy Attack atau Attack biasa
        bool isHeavy = anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack");

        // 2. Tentukan range dan damage berdasarkan jenis animasi yang sedang aktif
        float range = isHeavy ? heavyRange : swingRange;
        float finalDamage = isHeavy ? (attackDamage * 2f) : attackDamage;

        // 3. Deteksi musuh dalam radius attackPoint
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, range, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy);

            if (isHeavy)
            {
                if (angleToEnemy < 90f) ApplyDamage(enemy, finalDamage);
            }
            else
            {
                if (angleToEnemy < swingAngle / 2) ApplyDamage(enemy, finalDamage);
            }
        }
    }

    void ApplyDamage(Collider enemy, float damage)
    {
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            if (PlayerStats.instance.isRageMode)
            {
                float healAmount = damage; // 10% lifesteal
                PlayerStats.instance.currentHealth = Mathf.Clamp(PlayerStats.instance.currentHealth + healAmount, 0, PlayerStats.instance.maxHealth);
            }
        }
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

    IEnumerator DashRoutine()
    {
        isDashing = true;
        isImmune = true; // Mulai masa immune
        lastDashTime = Time.time;

        /*
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.isTrigger = true;
        */

        // RESET ANIMASI: Paksa kaki diam sebelum mulai dash
        anim.SetFloat("moveX", 0);
        anim.SetFloat("moveZ", 0);
        anim.SetBool("isWalking", false); // Jika kamu punya parameter ini

        // 1. Arahkan karakter ke kursor tepat saat dash dimulai
        // Logika LookAtMouse() sudah ada, jadi karakter akan otomatis menghadap kursor.
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
        PlayerStats.instance.currentRage = 0; // Reset bar

        // 1. Bonus Atribut: Speed & Animasi
        float originalSpeed = speed;
        speed *= 1.5f; // Lari lebih cepat
        anim.speed = rageAnimSpeed; // Animasi serangan & jalan lebih cepat

        Debug.Log("RAGE MODE AKTIF!");

        // 2. Durasi Rage (100 detik)
        float timer = rageDuration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            // Opsional: Kamu bisa membuat bar rage berkurang pelan-pelan sebagai timer visual
            PlayerStats.instance.currentRage = timer; 
            yield return null;
        }

        // 3. Rage Selesai: Pinalti Kelelahan
        PlayerStats.instance.isRageMode = false;
        speed = originalSpeed;
        anim.speed = 1f;

        PlayerStats.instance.currentMP = 0;
        PlayerStats.instance.currentEnergy = 0;
        
        Debug.Log("Rage berakhir. Player kelelahan!");
    }
}