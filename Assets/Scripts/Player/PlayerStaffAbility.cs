using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerStaffAbility : NetworkBehaviour
{
    private PlayerContext ctx;

    public NetworkVariable<float> CooldownRemaining =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<float> AbilityRemaining =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> IsActive =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    //==================================================//

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();
    }

    //==================================================//

    public override void OnNetworkSpawn()
    {
        if (ctx == null)
            return;

        if (ctx.inputEvents != null)
            ctx.inputEvents.AbilityPressed += TryUseAbility;
    }

    public override void OnNetworkDespawn()
    {
        if (ctx != null && ctx.inputEvents != null)
            ctx.inputEvents.AbilityPressed -= TryUseAbility;
    }

    //==================================================//

    private void Update()
    {
        if (!IsServer)
            return;

        UpdateCooldown();
        UpdateAbilityDuration();
    }

    //==================================================//

    private void TryUseAbility()
    {
        if (!IsOwner)
            return;

        if (ctx.health.IsDead.Value)
            return;

        if (ctx.equipment == null)
            return;

        Weapon weapon = ctx.equipment.weapon;

        if (weapon == null)
            return;

        if (weapon.ability == null)
            return;

        if (CooldownRemaining.Value > 0f)
            return;

        UseAbilityRpc();
    }

    //==================================================//

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Owner)]
    private void UseAbilityRpc()
    {
        if (ctx.health.IsDead.Value)
            return;

        Weapon weapon = ctx.equipment.weapon;

        if (weapon == null)
            return;

        StaffAbilityData ability = weapon.ability;

        if (ability == null)
            return;

        if (CooldownRemaining.Value > 0f)
            return;

        ActivateAbility(ability);
    }

    //==================================================//

    private void ActivateAbility(StaffAbilityData ability)
    {
        CooldownRemaining.Value = ability.cooldown;
        AbilityRemaining.Value = ability.duration;
        IsActive.Value = true;

        switch (ability.abilityType)
        {
            case StaffAbilityType.NatureShield:

                ctx.health.SetInvulnerable(true);

                break;
        }
    }

    //==================================================//

    private void UpdateCooldown()
    {
        if (CooldownRemaining.Value <= 0f)
            return;

        CooldownRemaining.Value =
            Mathf.Max(
                CooldownRemaining.Value - Time.deltaTime,
                0f
            );
    }

    //==================================================//

    private void UpdateAbilityDuration()
    {
        if (!IsActive.Value)
            return;

        AbilityRemaining.Value =
            Mathf.Max(
                AbilityRemaining.Value - Time.deltaTime,
                0f
            );

        if (AbilityRemaining.Value <= 0f)
        {
            DeactivateAbility();
        }
    }

    //==================================================//

    private void DeactivateAbility()
    {
        IsActive.Value = false;

        ctx.health.SetInvulnerable(false);
    }
}
