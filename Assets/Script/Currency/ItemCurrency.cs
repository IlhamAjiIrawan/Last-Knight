using UnityEngine;

public class ItemCurrency : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Gold";
    public int value = 10;

    [Header("Magnet Settings")]
    public float pullRadius = 5f;
    public float moveSpeed = 10f;
    public float magnetDelay = 3f;
    
    // Variabel ini harus private agar tidak tertukar di Inspector
    private Transform _playerTransform; 
    private Rigidbody _rb;
    private float _spawnTime;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _spawnTime = Time.time;

        if (PlayerStats.instance != null)
        {
            _playerTransform = PlayerStats.instance.transform;
            Debug.Log("Koin berhasil menemukan Player: " + _playerTransform.name);
        }
        else
        {
            // Jika Singleton gagal, cari otomatis lewat Tag (Plan B)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
                Debug.Log("Koin menemukan Player lewat Tag");
            }
            else
            {
                Debug.LogError("KOIN GAGAL MENEMUKAN PLAYER! Pastikan Player ada di Hierarchy dan memiliki script PlayerStats atau Tag 'Player'.");
            }
        }

        // Efek terpental sedikit saat baru muncul (agar tidak menumpuk)
        if (_rb != null)
        {
            Vector3 force = new Vector3(Random.Range(-2f, 2f), 5f, Random.Range(-2f, 2f));
            _rb.AddForce(force, ForceMode.Impulse);
        }
    }

    void Update()
    {
        if (Time.time < _spawnTime + magnetDelay) return;
        
        // Pastikan PlayerStats dan playerBody-nya sudah siap
        if (PlayerStats.instance == null || PlayerStats.instance.playerBody == null) return;
        Transform target = PlayerStats.instance.playerBody;
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= pullRadius)
        {
            if (_rb != null && !_rb.isKinematic) _rb.isKinematic = true;
            // Terbang menuju tubuh ksatria, bukan menuju objek kosong tadi
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up * 360f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cek tabrakan menggunakan Tag "Player" (Sangat penting)
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    void CollectItem()
    {
        if (PlayerStats.instance != null)
        {
            PlayerStats.instance.gold += value;
        }
        
        // Hancurkan koin setelah diambil
        Destroy(gameObject);
    }
}