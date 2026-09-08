using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject lobbyPlayerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        NetworkObject lobbyPlayer =
            Instantiate(lobbyPlayerPrefab);

        lobbyPlayer.SpawnWithOwnership(clientId);
    }
}
