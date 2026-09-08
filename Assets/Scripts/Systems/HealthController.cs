using System;
using Unity.Netcode;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;

    public NetworkVariable<float> CurrentHealth =
        new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> IsDead =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public float HealthPercent =>
        maxHealth > 0f ? CurrentHealth.Value / maxHealth : 0f;

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;

    private DeathController deathController;


    //==============================================================//
    // SPAWN
    //==============================================================//

    public override void OnNetworkSpawn()
    {
        deathController = GetComponent<DeathController>();

        if (IsServer)
            CurrentHealth.Value = maxHealth;

        CurrentHealth.OnValueChanged += OnHealthValueChanged;
    }


    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= OnHealthValueChanged;
    }


    //==============================================================//
    // HEALTH
    //==============================================================//

    private void OnHealthValueChanged(float previous, float current)
    {
        OnHealthChanged?.Invoke(current, maxHealth);
    }


    public void TakeDamage(float amount)
    {
        if (!IsServer || IsDead.Value)
            return;

        CurrentHealth.Value = Mathf.Max(
            CurrentHealth.Value - amount,
            0f
        );

        if (CurrentHealth.Value <= 0f)
            Die();
    }


    public void Heal(float amount)
    {
        if (!IsServer || IsDead.Value)
            return;

        CurrentHealth.Value = Mathf.Min(
            CurrentHealth.Value + amount,
            maxHealth
        );
    }


    //==============================================================//
    // DEATH
    //==============================================================//

    public virtual void Die()
    {
        if (!IsServer || IsDead.Value)
            return;

        IsDead.Value = true;

        deathController?.HandleDeath();

        OnDeath?.Invoke();
    }
}
