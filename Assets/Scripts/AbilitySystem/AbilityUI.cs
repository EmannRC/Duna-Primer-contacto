using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownText;

    private PlayerAbilitySystem abilitySystem;

    public void Bind(PlayerAbilitySystem system)
    {
        abilitySystem = system;
        Refresh();
    }

    private void Update() => Refresh();

    private void Refresh()
    {
        if (abilitySystem == null || abilitySystem.CurrentAbility == null)
        {
            Hide();
            return;
        }

        AbilityData data = abilitySystem.CurrentAbility;
        if (abilityIcon != null)
        {
            abilityIcon.sprite = data.icon;
            abilityIcon.enabled = data.icon != null;
            abilityIcon.gameObject.SetActive(true);
        }

        float cooldown = Mathf.Max(0f, data.cooldown);
        float remaining = Mathf.Clamp(abilitySystem.CooldownRemaining.Value, 0f, cooldown);
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = cooldown > 0f ? remaining / cooldown : 0f;
            cooldownFill.gameObject.SetActive(true);
        }

        if (cooldownText != null)
            cooldownText.text = remaining > 0f ? Mathf.CeilToInt(remaining).ToString() : string.Empty;
    }

    private void Hide()
    {
        if (abilityIcon != null) abilityIcon.gameObject.SetActive(false);
        if (cooldownFill != null) cooldownFill.gameObject.SetActive(false);
        if (cooldownText != null) cooldownText.text = string.Empty;
    }
}
