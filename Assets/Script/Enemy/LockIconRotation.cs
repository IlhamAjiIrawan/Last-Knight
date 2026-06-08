using UnityEngine;

public class LockIconRotation : MonoBehaviour
{
    // Menggunakan LateUpdate agar posisi rotasi diperbarui setelah pergerakan musuh selesai
    void LateUpdate()
    {
        // Memaksa rotasi icon selalu tetap flat menghadap langit (X:90) dan tidak ikut berputar (Y:0, Z:0)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}