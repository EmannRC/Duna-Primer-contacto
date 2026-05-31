using System;
using Unity.Netcode;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public NetworkVariable<float> currentHealth = new();

    [Header("Regeneración")]
    public bool canRegenerate = false;
    public float regenRate = 5f;

    public Action OnDeath;

    public Action<float, float> OnHealthChanged;

    //=========================================================//

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnHealthValueChanged;
    }
    void OnHealthValueChanged(
    float previous,
    float current)
    {
        OnHealthChanged?.Invoke(
            current,
            maxHealth);
    }

    void Update()
    {
        Regeneration(); //Esto es para pruebas
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer)
            return;

        currentHealth.Value -= amount;

        currentHealth.Value = Mathf.Max(currentHealth.Value,0f);

        Debug.Log(name +" recibió daño. Vida: " +currentHealth.Value);

        if (currentHealth.Value <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!IsServer)
            return;

        currentHealth.Value += amount;

        currentHealth.Value = Mathf.Min(currentHealth.Value,maxHealth);
    }

    private void Regeneration()
    {
        if (!IsServer)
            return;

        if (!canRegenerate)
            return;

        if (currentHealth.Value >= maxHealth)
            return;

        currentHealth.Value += regenRate * Time.deltaTime;

        currentHealth.Value =Mathf.Min(currentHealth.Value,maxHealth);
    }

    public virtual void Die()
    {
        Debug.Log(name + " murió");
        OnDeath?.Invoke();
    }
}
