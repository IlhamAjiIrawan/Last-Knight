using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillHUD : MonoBehaviour
{
    [Header("Referensi Player")]
    public PlayerMovement playerMovement;

    [System.Serializable]
    public struct SkillUIElements
    {
        public Image overlayImage;       // Gambar hitam transparan untuk efek cooldown berputar
        public TextMeshProUGUI cdText;   // Teks angka countdown (misal: 3, 2, 1)
        public TextMeshProUGUI lvlText;  // Teks level skill (misal: Lvl 1)
    }

    [Header("UI Slots Skill (1 sampai 4)")]
    public SkillUIElements skill1;
    public SkillUIElements skill2;
    public SkillUIElements skill3;
    public SkillUIElements skill4;

    void Update()
    {
        if (playerMovement == null || PlayerStats.instance == null) return;

        // Update masing-masing UI Skill secara real-time
        UpdateSlot(playerMovement.skill1CDTimer, playerMovement.skill1MaxCD, PlayerStats.instance.skill1Level, skill1);
        UpdateSlot(playerMovement.skill2CDTimer, playerMovement.skill2MaxCD, PlayerStats.instance.skill2Level, skill2);
        UpdateSlot(playerMovement.skill3CDTimer, playerMovement.skill3MaxCD, PlayerStats.instance.skill3Level, skill3);
        UpdateSlot(playerMovement.skill4CDTimer, playerMovement.skill4MaxCD, PlayerStats.instance.skill4Level, skill4);
    }

    void UpdateSlot(float currentCD, float maxCD, int currentLevel, SkillUIElements ui)
    {
        if (ui.lvlText != null)
        {
            ui.lvlText.text = currentLevel > 0 ? "Lvl " + currentLevel : "Locked";
        }

        if (currentLevel <= 0)
        {
            if (ui.overlayImage != null) ui.overlayImage.fillAmount = 1f;
            if (ui.cdText != null) ui.cdText.text = "";
            return;
        }

        if (currentCD > 0)
        {
            // CETAK LOG UNTUK MEMASTIKAN TIMER BERJALAN
            Debug.Log($"[HUD] Skill terdeteksi CD: {currentCD} / Max: {maxCD}");

            if (ui.overlayImage != null) ui.overlayImage.fillAmount = currentCD / maxCD;
            if (ui.cdText != null) ui.cdText.text = Mathf.CeilToInt(currentCD).ToString();
        }
        else
        {
            if (ui.overlayImage != null) ui.overlayImage.fillAmount = 0f;
            if (ui.cdText != null) ui.cdText.text = "";
        }
    }
}