using UnityEngine;

public class BossAnimationBridge : MonoBehaviour
{
    private KingBarbarian bossScript;

    void Start()
    {
        // Mencari script KingBarbarian di parent-nya
        bossScript = GetComponentInParent<KingBarbarian>();
    }

    // Event ini yang dipanggil oleh Animation Clip
    public void TriggerChopSlash()
    {
        if (bossScript != null)
        {
            bossScript.TriggerChopSlash();
        }
    }
}