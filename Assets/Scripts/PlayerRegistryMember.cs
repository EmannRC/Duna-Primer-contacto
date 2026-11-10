using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerContext))]
public class PlayerRegistryMember : NetworkBehaviour
{
    private PlayerContext player;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        player = GetComponent<PlayerContext>();
        PlayerRegistry.Register(player);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        if (player != null)
            PlayerRegistry.Unregister(player);
    }
}
