using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Membuat UI selalu menghadap ke kamera
        transform.LookAt(transform.position + cam.forward);
    }
}