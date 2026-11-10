using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FarmController farmController;
    [SerializeField] private Image radialFill;
    [SerializeField] private TMP_Text timerText;

    [Header("Settings")]
    [SerializeField] private bool showOnlyDuringCooldown = true;

    private void Update()
    {
        if (farmController == null || !farmController.IsSpawned)
        {
            SetVisible(false);
            return;
        }

        bool isCoolingDown =
            farmController.State == FarmController.ResourceState.Cooldown;

        if (!isCoolingDown)
        {
            SetVisible(!showOnlyDuringCooldown);
            return;
        }

        SetVisible(true);

        double remaining =
            farmController.RespawnAt -
            NetworkManager.Singleton.ServerTime.Time;

        remaining = System.Math.Max(0, remaining);

        float progress = 1f -
            (float)(remaining / farmController.CooldownDuration);

        if (radialFill != null)
            radialFill.fillAmount = Mathf.Clamp01(progress);

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt((float)remaining);

            int minutes = seconds / 60;
            int remainder = seconds % 60;

            timerText.text = $"{minutes:00}:{remainder:00}";
        }
    }

    private void SetVisible(bool visible)
    {
        if (radialFill != null)
            radialFill.transform.parent.gameObject.SetActive(visible);
    }
}
