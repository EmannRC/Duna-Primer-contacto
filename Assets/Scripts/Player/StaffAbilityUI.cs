using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffAbilityUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownText;

    private PlayerStaffAbility ability;
    private PlayerContext ctx;

    public void Bind(PlayerStaffAbility playerAbility)
    {
        ability = playerAbility;

        if (ability == null)
        {
            Hide();
            return;
        }

        ctx = ability.GetComponent<PlayerContext>();

        if (ctx != null && ctx.equipment != null)
        {
            ctx.equipment.OnEquipmentChanged += UpdateAbilityUI;
        }

        UpdateAbilityUI();
    }

    private void Update()
    {
        if (ability == null || ctx == null)
            return;

        Debug.Log($"UI Ability | Weapon: {ctx.equipment.weapon} | " + $"Ability: {ctx.equipment.weapon?.ability}");
        UpdateAbilityUI();
    }

    private void UpdateAbilityUI()
    {
        if (ctx.equipment == null)
        {
            Hide();
            return;
        }

        Weapon weapon = ctx.equipment.weapon;

        if (weapon == null || weapon.ability == null)
        {
            Hide();
            return;
        }

        Show();

        // ICONO
        if (abilityIcon != null)
        {
            abilityIcon.sprite = weapon.ability.icon;
        }

        // COOLDOWN
        float cooldown = weapon.ability.cooldown;
        float remaining = ability.CooldownRemaining.Value;

        if (cooldown <= 0f)
            return;

        if (cooldownText != null)
        {
            if (remaining > 0f)
                cooldownText.text = Mathf.CeilToInt(remaining).ToString();
            else
                cooldownText.text = "";
        }

        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = remaining / cooldown;
        }
    }

    private void Show()
    {
        if (abilityIcon != null)
            abilityIcon.gameObject.SetActive(true);

        if (cooldownFill != null)
            cooldownFill.gameObject.SetActive(true);
    }

    private void Hide()
    {
        if (abilityIcon != null)
            abilityIcon.gameObject.SetActive(false);

        if (cooldownFill != null)
            cooldownFill.gameObject.SetActive(false);

        if (cooldownText != null)
            cooldownText.text = "";
    }

    private void OnDestroy()
    {
        if (ctx != null && ctx.equipment != null)
        {
            ctx.equipment.OnEquipmentChanged -= UpdateAbilityUI;
        }
    }
}
