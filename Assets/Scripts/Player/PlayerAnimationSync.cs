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

        VelX.Value = localMove.x;
        VelY.Value = localMove.z;

        Speed.Value = ctx.movement.AnimationSpeed;

        Grounded.Value = ctx.movement.IsGrounded;

        Vertical.Value = ctx.movement.VerticalVelocity;

        Crouching.Value = ctx.movement.IsCrouching;
    }
}
