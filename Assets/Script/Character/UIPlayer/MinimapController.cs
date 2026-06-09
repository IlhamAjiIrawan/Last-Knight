using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Seret GameObject Panel Mini Map kamu ke sini")]
    public GameObject minimapPanel;

    [Header("Keybindings")]
    [Tooltip("Tombol keyboard yang digunakan untuk membuka/menutup mini map")]
    public KeyCode toggleKey = KeyCode.M;

    void Start()
    {
        // Validasi awal agar tidak terjadi NullReferenceException
        if (minimapPanel == null)
        {
            Debug.LogError("[MinimapController]: Game Object 'minimapPanel' belum dipasang di Inspector!");
        }
    }

    void Update()
    {
        // Mendeteksi jika player menekan tombol shortcut di keyboard
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMinimap();
        }
    }

    // Fungsi ini dibuat 'public' agar bisa dipanggil juga oleh UI Button (OnClick event)
    public void ToggleMinimap()
    {
        if (minimapPanel != null)
        {
            // Membalikkan status aktif panel (jika true jadi false, jika false jadi true)
            bool currentState = minimapPanel.activeSelf;
            minimapPanel.SetActive(!currentState);

            // Log opsional untuk debugging di console
            Debug.Log($"🗺️ Mini Map: {(!currentState ? "TERBUKA" : "TERTUTUP")}");
        }
    }
}