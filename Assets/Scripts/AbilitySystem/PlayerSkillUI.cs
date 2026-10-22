using Duna.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duna.UI
{
    public class PlayerSkillUI : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private PlayerSkillSystem skillSystem;

        [Header("Energia")]
        [SerializeField] private Image energyFill;
        [SerializeField] private TMP_Text energyText;

        [Header("Celeridad - Q")]
        [SerializeField] private Image celerityCooldownFill;
        [SerializeField] private TMP_Text celerityCooldownText;

        [Header("Impulso - Espacio")]
        [SerializeField] private Image impulseCooldownFill;
        [SerializeField] private TMP_Text impulseCooldownText;

        [Header("Repeler - X")]
        [SerializeField] private Image repelCooldownFill;
        [SerializeField] private TMP_Text repelCooldownText;

        [Header("Costes de energia")]
        [SerializeField] private int celerityCost = 2;
        [SerializeField] private int impulseCost = 4;
        [SerializeField] private int repelCost = 8;

        [Header("Duracion de cooldown")]
        [SerializeField] private float celerityCooldown = 1f;
        [SerializeField] private float impulseCooldown = 3f;
        [SerializeField] private float repelCooldown = 3f;


        public void Bind(PlayerSkillSystem system)
        {
            if (system == null)
            {
                Debug.LogError("[PlayerSkillUI] PlayerSkillSystem es null.");
                return;
            }

            skillSystem = system;

            Debug.Log("[PlayerSkillUI] Vinculado al PlayerSkillSystem del jugador local.");
        }

        private void Update()
        {
            if (skillSystem == null)
                return;

            UpdateEnergy();
            UpdateSkill(
                celerityCooldownFill,
                celerityCooldownText,
                skillSystem.CelerityCooldownRemaining,
                celerityCooldown,
                celerityCost
            );

            UpdateSkill(
                impulseCooldownFill,
                impulseCooldownText,
                skillSystem.ImpulseCooldownRemaining,
                impulseCooldown,
                impulseCost
            );

            UpdateSkill(
                repelCooldownFill,
                repelCooldownText,
                skillSystem.RepelCooldownRemaining,
                repelCooldown,
                repelCost
            );
        }

        private void UpdateEnergy()
        {
            int current = skillSystem.CurrentEnergy;
            int max = skillSystem.MaxEnergy;

            if (energyText != null)
                energyText.text = $"{current} / {max}";

            if (energyFill != null)
            {
                energyFill.fillAmount = max > 0
                    ? Mathf.Clamp01((float)current / max)
                    : 0f;
            }
        }

        private void UpdateSkill(
            Image cooldownFill,
            TMP_Text cooldownText,
            float remaining,
            float duration,
            int cost)
        {
            bool onCooldown = remaining > 0f;
            bool enoughEnergy = skillSystem.CurrentEnergy >= cost;

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = duration > 0f
                    ? Mathf.Clamp01(remaining / duration)
                    : 0f;

                cooldownFill.enabled = onCooldown;
            }

            if (cooldownText != null)
            {
                cooldownText.text = onCooldown
                    ? remaining.ToString("0.0")
                    : enoughEnergy ? "" : "Sin energia";
            }
        }

        public void SetSkillSystem(PlayerSkillSystem system)
        {
            skillSystem = system;
        }
    }
}
