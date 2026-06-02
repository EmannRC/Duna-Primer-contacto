using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Bars")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image manaFill;
    [SerializeField] private Image xpFill;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelText;

    //References
    private PlayerHealth health;
    private PlayerMana mana;
    private PlayerLevelSystem levelSystem;
    private PlayerStatsManager stats;


    //=========================================================//
    public void Bind(Transform playerRoot)
    {
        Unbind();

        PlayerContext ctx = playerRoot.GetComponent<PlayerContext>();

        health = ctx.health;
        mana = ctx.mana;
        levelSystem = ctx.levelSystem;
        stats = ctx.stats;

        if (health != null)
            health.OnHealthChanged += OnHealthChanged;

        if (mana != null)
            mana.OnManaChanged += Refresh;

        if (levelSystem != null)
            levelSystem.OnXpChanged += Refresh;

        if (stats != null)
            stats.OnStatsChanged += Refresh;

        Refresh();
    }

    //=========================================================//
    void Unbind()
    {
        if (health != null)
            health.OnHealthChanged -= OnHealthChanged;

        if (mana != null)
            mana.OnManaChanged -= Refresh;

        if (levelSystem != null)
            levelSystem.OnXpChanged -= Refresh;

        if (stats != null)
            stats.OnStatsChanged -= Refresh;
    }

    //=========================================================//
    void OnDestroy()
    {
        Unbind();
    }

    //=========================================================//
    void OnHealthChanged(
        float previous,
        float current)
    {
        Refresh();
    }

    //=========================================================//
    void Refresh()
    {
        UpdateHealth();
        UpdateMana();
        UpdateXP();
        UpdateTexts();
    }

    //=========================================================//
    void UpdateHealth()
    {
        float value = health.HealthPercent;

        healthFill.fillAmount = value;

        UpdateHealthColor(value);
    }

    //=========================================================//
    void UpdateMana()
    {
        manaFill.fillAmount = mana.GetPercent();
    }

    //=========================================================//
    void UpdateXP()
    {
        xpFill.fillAmount = levelSystem.GetXpNormalized();
    }

    //=========================================================//
    void UpdateTexts()
    {
        levelText.text = $"Nivel: {levelSystem.level.Value} / Poder: {stats.GetStat(StatType.Power):0}";
    }

    //=========================================================//
    void UpdateHealthColor(float percent)
    {
        if (percent > .6f)
            healthFill.color = Color.green;
        else if (percent > .3f)
            healthFill.color = Color.yellow;
        else
            healthFill.color = Color.red;
    }
}
