using UnityEngine;
using System.Collections;

public class IceSlowTrap : MonoBehaviour
{
    [Header("Slow Settings")]
    [Range(0.1f, 0.9f)]
    public float slowMultiplier = 0.1f; // Mengurangi speed sebanyak 50%

    [Header("VFX Freeze Settings (Howl Studio - Particle Only)")]
    [Tooltip("Waktu dalam detik sampai es muncul penuh sebelum dibekukan (Saran: 0.44)")]
    public float timeToFullyAppear = 0.44f; 

    private ParticleSystem parentParticle;

    private void Start()
    {
        parentParticle = GetComponent<ParticleSystem>();
        StartCoroutine(FreezeParticleRoutine());
    }

    private IEnumerator FreezeParticleRoutine()
    {
        yield return new WaitForSeconds(timeToFullyAppear);

        if (parentParticle != null)
        {
            parentParticle.Pause(true); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Player"))
        {
            var movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.trapSpeedMultiplier = slowMultiplier;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.trapSpeedMultiplier = 1f;
            }
        }
    }

    private void OnDestroy()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var movement = playerObj.GetComponent<PlayerMovement>();
            if (movement != null && movement.trapSpeedMultiplier == slowMultiplier)
            {
                movement.trapSpeedMultiplier = 1f;
            }
        }
    }
}