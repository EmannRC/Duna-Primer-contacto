using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerRespawn : NetworkBehaviour
{
    private PlayerContext ctx;
    private PlayerSpawner spawner;

    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();
    }


    //========================================================//
    // NETWORK SPAWN
    //========================================================//

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            spawner =
                FindFirstObjectByType<PlayerSpawner>();
        }
    }


    //========================================================//
    // REQUEST RESTART
    //========================================================//

    public void RequestRestart()
    {
        Debug.Log(
            $"[RESPAWN] RequestRestart | IsOwner={IsOwner}"
        );

        if (!IsOwner)
            return;

        RequestRestartRpc();
    }


    //========================================================//
    // SERVER
    //========================================================//

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Owner
    )]
    private void RequestRestartRpc()
    {
        Debug.Log(
            $"[RESPAWN] Server recibió restart | " +
            $"ClientId={OwnerClientId}"
        );

        if (ctx == null || ctx.health == null)
        {
            Debug.LogError(
                "[RESPAWN] ctx o health es NULL."
            );

            return;
        }

        if (!ctx.health.IsDead.Value)
        {
            Debug.LogWarning(
                "[RESPAWN] El jugador no está muerto."
            );

            return;
        }

        if (spawner == null)
        {
            spawner =
                FindFirstObjectByType<PlayerSpawner>();
        }

        if (spawner == null)
        {
            Debug.LogError(
                "[RESPAWN] No se encontró PlayerSpawner."
            );

            return;
        }

        Vector3 spawnPosition =
            spawner.GetSpawnPosition(OwnerClientId);

        Quaternion spawnRotation =
            spawner.GetSpawnRotation(OwnerClientId);

        Debug.Log(
            $"[RESPAWN] Respawneando ClientId={OwnerClientId} | " +
            $"Pos={spawnPosition}"
        );


        //====================================================//
        // RESET HEALTH
        //====================================================//

        ctx.health.ResetHealth();


        //====================================================//
        // RESET DEATH CONTROLLER
        //====================================================//

        DeathController death =
            GetComponent<DeathController>();

        if (death != null)
        {
            death.ResetDeathState();
        }


        //====================================================//
        // AVISAR AL OWNER
        //====================================================//

        RespawnOwnerRpc(
            spawnPosition,
            spawnRotation
        );
    }


    //========================================================//
    // OWNER
    //========================================================//

    [Rpc(SendTo.Owner)]
    private void RespawnOwnerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log(
            $"[RESPAWN] Owner recibió respawn | " +
            $"Pos={position}"
        );

        if (!IsOwner)
            return;

        if (ctx == null)
        {
            Debug.LogError(
                "[RESPAWN] PlayerContext es NULL."
            );

            return;
        }

        CharacterController controller =
            ctx.controller;


        //====================================================//
        // TELEPORT
        //====================================================//

        if (controller != null)
            controller.enabled = false;

        transform.SetPositionAndRotation(
            position,
            rotation
        );

        if (controller != null)
            controller.enabled = true;


        //====================================================//
        // RESET MOVEMENT
        //====================================================//

        if (ctx.movement != null)
        {
            ctx.movement.ResetForRespawn();
        }


        //====================================================//
        // RESET COMBAT
        //====================================================//

        if (ctx.combat != null)
        {
            ctx.combat.EndAttack();
        }

        //====================================================//
        // REVIVE ANIMATION
        //====================================================//

        if (ctx.playerAnimation != null)
        {
            ctx.playerAnimation.PlayReviveAnimation();
        }

        //====================================================//
        // HIDE DEATH MENU
        //====================================================//

        if (LocalPlayerBootstrap.Instance != null)
        {
            LocalPlayerBootstrap.Instance.HideDeathMenu();
        }


        //====================================================//
        // DEBUG
        //====================================================//

        Debug.Log(
            $"[RESPAWN] Respawn COMPLETADO | " +
            $"Pos actual={transform.position}"
        );
    }
}
