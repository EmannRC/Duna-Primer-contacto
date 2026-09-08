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
        if (!IsOwner)
            return;

        RestartServerRpc();
    }


    //========================================================//
    // SERVER
    //========================================================//

    [ServerRpc]
    private void RestartServerRpc(ServerRpcParams rpcParams = default)
    {
        // Seguridad: asegurarnos de que la petición
        // viene del propietario de este Player.
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (ctx == null || ctx.health == null)
            return;

        if (!ctx.health.IsDead.Value)
            return;

        PlayerSpawner spawner =
            FindFirstObjectByType<PlayerSpawner>();

        if (spawner == null)
        {
            Debug.LogError(
                "PlayerRespawn: No se encontró PlayerSpawner."
            );

            return;
        }

        // Reposicionar.
        spawner.RespawnPlayer(OwnerClientId);

        // Restaurar vida.
        ctx.health.ResetHealth();

        // Avisar al jugador propietario.
        RespawnClientRpc();
    }


    //========================================================//
    // CLIENT
    //========================================================//

    [ClientRpc]
    private void RespawnClientRpc(
        ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner)
            return;

        if (ctx == null)
            return;

        // Desbloquear movimiento.
        if (ctx.movement != null)
            ctx.movement.SetMovementLocked(false);

        // Restaurar estado de ataque.
        if (ctx.combat != null)
            ctx.combat.EndAttack();
    }
}
