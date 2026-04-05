using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody rb;
    private Camera mainCamera;
    private Animator anim; // Tambahkan ini
    public float attackDamage = 20f;
    public float attackRange = 1.5f;
    public Transform attackPoint; // Titik di depan pedang
    public LayerMask enemyLayers; // Pilih layer "Enemy" di Inspector

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // Ambil komponen Animator
        mainCamera = Camera.main;

        // Ambil speed dan damage dari stats
        speed = PlayerStats.instance.speed;
        attackDamage = PlayerStats.instance.damage;
    }

    void Update()
    {
        LookAtMouse();
        UpdateAnimation(); // Panggil fungsi animasi setiap frame
        if (Input.GetMouseButtonDown(0)) // 0 adalah Klik Kiri
        {
            Attack();
        }
    }

    void UpdateAnimation()
    {
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Melee_1H_Attack_Slice_Diagonal");

        if (isAttacking)
        {
            // Jika sedang menyerang, paksa parameter Move ke 0 agar animasi kaki diam
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
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Melee_1H_Attack_Slice_Diagonal");

        if (isAttacking)
        {
            // Hentikan semua kecepatan gerak saat menyerang
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
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

    void Attack()
    {
        // Memicu animasi serangan
        anim.SetTrigger("attack");

        // Deteksi musuh dalam jangkauan serangan
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, attackRange, enemyLayers);

        // Berikan damage ke setiap musuh yang terkena
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.GetComponent<Health>())
            {
                enemy.GetComponent<Health>().TakeDamage(attackDamage);
            }
        }
    }

    // Untuk melihat jangkauan serangan di Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }
}