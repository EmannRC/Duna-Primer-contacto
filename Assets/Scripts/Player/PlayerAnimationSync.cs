using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimationSync : NetworkBehaviour
{
    private PlayerContext ctx;

    public NetworkVariable<float> Speed =
        new(0, default, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> VelX =
        new(0, default, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> VelY =
        new(0, default, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> Grounded =
        new(false, default, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> Vertical =
        new(0, default, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> Crouching =
        new(false, default, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> Dead =
        new(false, default, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> ShootCounter =
        new(0, default, NetworkVariableWritePermission.Owner);

    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        Vector3 move = ctx.movement.MoveDirection;

        Vector3 localMove =
            ctx.transform.InverseTransformDirection(move);

        UpdateFloat(VelX, localMove.x);
        UpdateFloat(VelY, localMove.z);

        UpdateFloat(Speed, ctx.movement.AnimationSpeed);

        if (Grounded.Value != ctx.movement.IsGrounded)
            Grounded.Value = ctx.movement.IsGrounded;

        UpdateFloat(
            Vertical,
            ctx.movement.VerticalVelocity);

        if (Crouching.Value != ctx.movement.IsCrouching)
            Crouching.Value = ctx.movement.IsCrouching;
    }

    private void UpdateFloat(
        NetworkVariable<float> variable,
        float value)
    {
        if (Mathf.Abs(variable.Value - value) < 0.01f)
            return;

        variable.Value = value;
    }

    public void NotifyShoot()
    {
        if (!IsOwner)
            return;

        ShootCounter.Value++;
    }
}
