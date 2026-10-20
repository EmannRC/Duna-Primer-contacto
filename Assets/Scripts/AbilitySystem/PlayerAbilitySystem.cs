using Unity.Netcode;
using UnityEngine;

public class PlayerAbilitySystem : NetworkBehaviour
{
    [SerializeField] private AbilityData abilityData;

    public readonly NetworkVariable<float> CooldownRemaining = new(0f);
    public readonly NetworkVariable<float> AbilityRemaining = new(0f);
    public readonly NetworkVariable<bool> IsActive = new(false);

    private PlayerContext ctx;

    public AbilityData CurrentAbility => abilityData;

    private void Awake() => ctx = GetComponent<PlayerContext>();

    public override void OnNetworkSpawn()
    {
        if (ctx != null && ctx.inputEvents != null)
            ctx.inputEvents.AbilityPressed += TryUseAbility;

        if (IsServer)
            SetShieldVisualAndState(false);
    }

    public override void OnNetworkDespawn()
    {
        if (ctx != null && ctx.inputEvents != null)
            ctx.inputEvents.AbilityPressed -= TryUseAbility;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        CooldownRemaining.Value = Mathf.Max(0f, CooldownRemaining.Value - Time.deltaTime);
        if (!IsActive.Value)
            return;

        AbilityRemaining.Value = Mathf.Max(0f, AbilityRemaining.Value - Time.deltaTime);
        if (AbilityRemaining.Value <= 0f)
            DeactivateAbility();
    }

    private void TryUseAbility()
    {
        if (!IsOwner || abilityData == null || ctx == null || ctx.health == null || ctx.health.IsDead.Value || CooldownRemaining.Value > 0f)
            return;

        UseAbilityRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void UseAbilityRpc()
    {
        if (abilityData == null || ctx == null || ctx.health == null || ctx.health.IsDead.Value || CooldownRemaining.Value > 0f)
            return;

        CooldownRemaining.Value = Mathf.Max(0f, abilityData.cooldown);
        AbilityRemaining.Value = Mathf.Max(0f, abilityData.duration);
        IsActive.Value = true;

        if (abilityData.abilityType == AbilityType.Shield)
            SetShieldVisualAndState(true);

        if (AbilityRemaining.Value <= 0f)
            DeactivateAbility();
    }

    private void DeactivateAbility()
    {
        IsActive.Value = false;
        AbilityRemaining.Value = 0f;
        SetShieldVisualAndState(false);
    }

    private void SetShieldVisualAndState(bool active)
    {
        if (ctx != null && ctx.health != null)
            ctx.health.SetInvulnerable(active);

        SetShieldVisualRpc(active);
    }

    [Rpc(SendTo.Everyone)]
    private void SetShieldVisualRpc(bool active)
    {
        PlayerAbilityEffects effects = GetComponent<PlayerAbilityEffects>();
        if (effects != null)
            effects.SetShieldVisible(active);
    }
}
