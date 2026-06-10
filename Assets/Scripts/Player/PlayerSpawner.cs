using Unity.Netcode;
using UnityEngine;

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
}

