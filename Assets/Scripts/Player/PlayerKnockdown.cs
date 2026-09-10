using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerKnockdown : NetworkBehaviour
{
    [Header("Knockdown")]
    [SerializeField] private float knockdownDuration = 2f;

    private PlayerContext ctx;

    private bool isKnockedDown;


    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();
    }


    //========================================================//
    // APPLY KNOCKDOWN
    //========================================================//

    public void ApplyKnockdown(Vector3 force)
    {
        if (!IsServer)
            return;

        if (isKnockedDown)
            return;

        isKnockedDown = true;

        KnockdownOwnerRpc(force);

        StartCoroutine(KnockdownRoutine());
    }


    //========================================================//
    // KNOCKDOWN ROUTINE
    //========================================================//

    private IEnumerator KnockdownRoutine()
    {
        yield return new WaitForSeconds(
            knockdownDuration
        );

        isKnockedDown = false;

        StandUpOwnerRpc();
    }


    //========================================================//
    // OWNER - KNOCKDOWN
    //========================================================//

    [Rpc(SendTo.Owner)]
    private void KnockdownOwnerRpc(Vector3 force)
    {
        if (!IsOwner)
            return;

        if (ctx == null)
            return;

        if (ctx.movement != null)
        {
            ctx.movement.SetMovementLocked(true);
            //ctx.movement.ApplyKnockback(force);
        }

        if (ctx.playerAnimation != null)
        {
            //ctx.playerAnimation.PlayKnockdownAnimation();
        }
    }


    //========================================================//
    // OWNER - STAND UP
    //========================================================//

    [Rpc(SendTo.Owner)]
    private void StandUpOwnerRpc()
    {
        if (!IsOwner)
            return;

        if (ctx == null)
            return;

        if (ctx.movement != null)
        {
            ctx.movement.SetMovementLocked(false);
        }

        if (ctx.playerAnimation != null)
        {
            //ctx.playerAnimation.PlayStandUpAnimation();
        }
    }
}
