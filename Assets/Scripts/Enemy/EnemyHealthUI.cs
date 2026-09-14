using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;

    private EnemyHealth health;

    //==============================================================//

    private void Awake()
    {
        health = GetComponentInParent<EnemyHealth>();
    }

    //==============================================================//

    private void OnEnable()
    {
        Subscribe();
    }

    //==============================================================//

    private void OnDisable()
    {
        Unsubscribe();
    }

    //==============================================================//

    private void Start()
    {
        // Start ocurre cuando todos los Awake del prefab ya terminaron.
        // Así la UI no depende del orden en que Unity inicialice componentes.
        Subscribe();
        Refresh();
    }

    //==============================================================//

    private void Subscribe()
    {
        if (health == null)
            health = GetComponentInParent<EnemyHealth>();

        if (health == null)
            return;

        health.OnHealthChanged -= UpdateHealth;
        health.OnHealthChanged += UpdateHealth;
        Refresh();
    }

    private void Unsubscribe()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateHealth;
    }

    private void Refresh()
    {
        if (health != null)
            UpdateHealth(health.CurrentHealth.Value, health.MaxHealth);
    }

    //==============================================================//

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFill == null)
            return;

        healthFill.fillAmount = Mathf.Clamp01(
            maxHealth > 0f
                ? currentHealth / maxHealth
                : 0f);
    }
}
