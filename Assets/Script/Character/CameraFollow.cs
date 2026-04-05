using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Drag karaktermu ke sini di Inspector
    public Vector3 offset = new Vector3(0, 10, -10); // Jarak aman kamera
    public float smoothSpeed = 0.125f; // Kecepatan gerak kamera agar halus

    void LateUpdate()
    {
        // Menghitung posisi tujuan berdasarkan posisi karakter + jarak (offset)
        Vector3 desiredPosition = target.position + offset;
        
        // Membuat gerakan kamera lebih halus (Slerp/Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        transform.position = smoothedPosition;

        // Opsional: Kamera selalu menghadap ke arah karakter
        // transform.LookAt(target); 
    }
}