using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private NetworkObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    //========================================================//
    // NETWORK SPAWN
    //========================================================//

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        // 1. Escuchar cuando los clientes terminan de cargar la escena
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadedForClients;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // 2. Spawnear a los que ya estén conectados (el Host)
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

        if (NetworkManager.Singleton.SceneManager != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadedForClients;

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }


    private void OnSceneLoadedForClients(
    string sceneName,
    UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
    System.Collections.Generic.List<ulong> clientsCompleted,
    System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in clientsCompleted)
        {
            SpawnPlayer(clientId);
        }
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

        if (playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner: Player Prefab no asignado.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerSpawner: No hay Spawn Points configurados.");
            return;
        }

        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        Transform spawnPoint = spawnPoints[spawnIndex];

        if (spawnPoint == null)
        {
            Debug.LogError($"PlayerSpawner: Spawn Point {spawnIndex} está vacío.");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        NetworkObject player = Instantiate(
            playerPrefab,
            spawnPosition,
            spawnRotation
        );

        player.SpawnAsPlayerObject(clientId);

        Debug.Log(
            $"Player {clientId} spawneado | " +
            $"Spawn: {spawnPoint.name} | " +
            $"Posición: {spawnPosition}"
        );
    }

    //========================================================//
    // GET SPAWN POSITION
    //========================================================//

    public Vector3 GetSpawnPosition(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PlayerSpawner: No hay Spawn Points.");
            return Vector3.zero;
        }

        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        Transform spawnPoint = spawnPoints[spawnIndex];

        if (spawnPoint == null)
            return Vector3.zero;

        return spawnPoint.position;
    }

    //========================================================//
    // GET SPAWN ROTATION
    //========================================================//

    public Quaternion GetSpawnRotation(ulong clientId)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return Quaternion.identity;

        int spawnIndex =
            (int)(clientId % (ulong)spawnPoints.Length);

        Transform spawnPoint = spawnPoints[spawnIndex];

        if (spawnPoint == null)
            return Quaternion.identity;

        return spawnPoint.rotation;
    }
}

