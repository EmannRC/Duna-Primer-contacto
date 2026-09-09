using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerRespawn : NetworkBehaviour
{
    private PlayerContext ctx;


    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();
    }


    //========================================================//
    // REQUEST
    //========================================================//

    public void RequestRestart()
    {
        Debug.Log($"[RESPAWN] RequestRestart | IsOwner={IsOwner}");

        if (!IsOwner)
            return;

        RestartServerRpc();
    }

    [ServerRpc]
    private void RestartServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log(
            $"[RESPAWN] ServerRpc recibido | " +
            $"Sender={rpcParams.Receive.SenderClientId} | " +
            $"Owner={OwnerClientId}"
        );

        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (ctx == null || ctx.health == null)
        {
            Debug.LogError("[RESPAWN] ctx o health es NULL.");
            return;
        }

        Debug.Log(
            $"[RESPAWN] IsDead antes del respawn = " +
            $"{ctx.health.IsDead.Value}"
        );

        if (!ctx.health.IsDead.Value)
        {
            Debug.LogWarning("[RESPAWN] El jugador NO está muerto.");
            return;
        }

        PlayerSpawner spawner =
            FindFirstObjectByType<PlayerSpawner>();

        if (spawner == null)
        {
            Debug.LogError("[RESPAWN] No se encontró PlayerSpawner.");
            return;
        }

        Debug.Log("[RESPAWN] Llamando a RespawnPlayer()");

        spawner.RespawnPlayer(OwnerClientId);

        Debug.Log(
            $"[RESPAWN] Posición después de RespawnPlayer: " +
            $"{transform.position}"
        );

        ctx.health.ResetHealth();

        DeathController death =
            GetComponent<DeathController>();

        if (death != null)
            death.ResetDeathState();

        Debug.Log(
            $"[RESPAWN] Respawn terminado | " +
            $"Pos={transform.position} | " +
            $"Health={ctx.health.CurrentHealth.Value} | " +
            $"IsDead={ctx.health.IsDead.Value}"
        );

        RespawnClientRpc();
    }

    //========================================================//
    // CLIENT
    //========================================================//

    [ClientRpc]
    private void RespawnClientRpc()
    {
        if (!IsOwner)
            return;

        if (ctx == null)
            return;

        if (ctx.movement != null)
            ctx.movement.SetMovementLocked(false);

        if (ctx.combat != null)
            ctx.combat.EndAttack();

        if (ctx.playerAnimation != null)
            ctx.playerAnimation.ResetDeathAnimation();
    }
}
