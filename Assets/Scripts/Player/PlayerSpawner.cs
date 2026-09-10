using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    //========================================================//
    // NETWORK SPAWN
    //========================================================//

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback
            += OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback
            += OnClientDisconnected;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }


    //========================================================//
    // NETWORK DESPAWN
    //========================================================//

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback
            -= OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback
            -= OnClientDisconnected;
    }


    //========================================================//
    // CLIENT CONNECTED
    //========================================================//

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayer(clientId);
    }


    //========================================================//
    // CLIENT DISCONNECTED
    //========================================================//

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Player desconectado: {clientId}");
    }


    //========================================================//
    // SPAWN PLAYER
    //========================================================//

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer)
            return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(
            clientId,
            out NetworkClient client))
        {
            if (client.PlayerObject != null &&
                client.PlayerObject.IsSpawned)
            {
                Debug.Log($"Player {clientId} ya tiene PlayerObject.");
                return;
            }
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerSpawner: No hay Spawn Points configurados.");
            return;
        }

        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        Transform spawnPoint =
            spawnPoints[spawnIndex];

        NetworkObject player =
            Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        player.SpawnAsPlayerObject(clientId);

        Debug.Log(
            $"Player {clientId} spawneado en " +
            $"{spawnPoint.name} | " +
            $"Pos: {spawnPoint.position}"
        );
    }


    //========================================================//
    // RESPAWN PLAYER
    //========================================================//

    public Vector3 GetSpawnPosition(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "PlayerSpawner: No hay Spawn Points configurados."
            );

            return Vector3.zero;
        }

        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        return spawnPoints[spawnIndex].position;
    }


    public Quaternion GetSpawnRotation(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return Quaternion.identity;
        }

        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        return spawnPoints[spawnIndex].rotation;
    }


    //========================================================//
    // RESPAWN ALL
    //========================================================//

    public void RespawnAllPlayers()
    {
        if (!IsServer)
            return;

        foreach (NetworkClient client
                 in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            if (!client.PlayerObject.IsSpawned)
                continue;

            //RespawnPlayer(client.ClientId);
        }
    }
}

