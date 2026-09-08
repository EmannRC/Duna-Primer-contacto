using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SubmitNameServerRpc(
                PlayerProfile.PlayerName);
        }
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string playerName)
    {
        PlayerName.Value = playerName;
    }
}
