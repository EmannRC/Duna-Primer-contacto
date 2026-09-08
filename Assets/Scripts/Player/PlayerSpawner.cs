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

        // Spawnear los clientes que ya estaban conectados
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

        // Evitar spawn duplicado
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


        // Comprobar Spawn Points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerSpawner: No hay Spawn Points configurados.");

            return;
        }


        // Elegir Spawn Point
        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        Transform spawnPoint =
            spawnPoints[spawnIndex];


        // Instanciar jugador
        NetworkObject player =
            Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );


        // Spawn como Player Object
        player.SpawnAsPlayerObject(clientId);


        Debug.Log(
            $"Player {clientId} spawneado en " +
            $"{spawnPoint.name} | " +
            $"Pos: {spawnPoint.position}"
        );
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

            client.PlayerObject.Despawn(true);
        }

        StartCoroutine(RespawnNextFrame());
    }


    //========================================================//
    // RESPAWN NEXT FRAME
    //========================================================//

    private IEnumerator RespawnNextFrame()
    {
        yield return null;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }
}

