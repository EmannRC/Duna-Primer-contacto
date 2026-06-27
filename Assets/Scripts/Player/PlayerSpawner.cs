using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        NetworkObject player =
            Instantiate(
                playerPrefab,
                spawnPoints[clientId % (ulong)spawnPoints.Length].position,
                Quaternion.identity);

        player.SpawnAsPlayerObject(clientId);
    }

    public void RespawnAllPlayers()
    {
        if (!IsServer) return;

        // 1. destruir players actuales de forma segura
        var players = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);

        foreach (var p in players)
        {
            if (p != null && p.IsPlayerObject && p.IsSpawned)
            {
                p.Despawn(true);
            }
        }

        // 2. esperar 1 frame lógico de seguridad (IMPORTANTE)
        StartCoroutine(RespawnNextFrame());
    }

    private IEnumerator RespawnNextFrame()
    {
        yield return null; // deja que NGO limpie despawns

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }
}

