using System;
using Unity.Netcode;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;

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

    public NetworkVariable<int> DeathTrigger =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public float HealthPercent => CurrentHealth.Value / maxHealth;


    public Action OnDeath;

    public Action<float, float> OnHealthChanged;

    //==============================================================//

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
        }

        CurrentHealth.OnValueChanged += OnHealthValueChanged;
    }

    //==============================================================//
    private void OnHealthValueChanged(float previous,float current)
    {
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    //==============================================================//
    public void TakeDamage(float amount)
    {
        if (!IsServer || IsDead.Value)
            return;

        CurrentHealth.Value -= amount;
        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value, 0);

        if (CurrentHealth.Value <= 0)
            Die();

    }

    //==============================================================//
    public void Heal(float amount)
    {
        if (!IsServer)
            return;

        if (IsDead.Value)
            return;

        CurrentHealth.Value += amount;

        CurrentHealth.Value = Mathf.Min(CurrentHealth.Value, maxHealth);
    }

    //==============================================================//
    public virtual void Die()
    {
        if (IsDead.Value)
            return;

        IsDead.Value = true;
        DeathTrigger.Value++;

        OnDeath?.Invoke();
    }
}
