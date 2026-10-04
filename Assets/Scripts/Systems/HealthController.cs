using System;
using Unity.Netcode;
using UnityEngine;

public class HealthController : NetworkBehaviour
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    public float HealthPercent =>
        maxHealth > 0f ? CurrentHealth.Value / maxHealth : 0f;

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;


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

    public NetworkVariable<bool> IsInvulnerable =
    new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Audio")]
    [SerializeField] private AudioSource damageSound;
    [SerializeField] private AudioClip damageClip;


    private DeathController deathController;

    private bool isInvulnerable;

    


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

        if (isInvulnerable)
            return;

        CurrentHealth.Value = Mathf.Max(
            CurrentHealth.Value - amount,
            0f
        );

        if(damageClip != null)
        PlayDamageSoundRpc();

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

    //==============================================================//
    // RESET
    //==============================================================//
    public void ResetHealth()
    {
        if (!IsServer)
            return;

        CurrentHealth.Value = maxHealth;
        IsDead.Value = false;
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void PlayDamageSoundRpc()
    {
        if (damageSound == null || damageClip == null)
            return;

        damageSound.PlayOneShot(damageClip);
    }

    public void SetInvulnerable(bool value)
    {
        if (!IsServer)
            return;

        isInvulnerable = value;
    }
}
