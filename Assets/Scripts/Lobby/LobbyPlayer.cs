using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnNameChanged;

        if (LobbyManager.Instance != null)
            LobbyManager.Instance.RegisterPlayer(this);

        if (IsOwner)
        {
            SubmitPlayerNameRpc(new FixedString64Bytes(PlayerProfile.PlayerName));
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnNameChanged;

        if (LobbyManager.Instance != null)
            LobbyManager.Instance.UnregisterPlayer(this);
    }

    private void OnNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.RefreshUI();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void SubmitPlayerNameRpc(FixedString64Bytes playerName)
    {
        string validatedName = playerName.ToString().Trim();

        if (string.IsNullOrWhiteSpace(validatedName))
            validatedName = "Jugador";

        if (validatedName.Length > 8)
            validatedName = validatedName.Substring(0, 8);

        PlayerName.Value = new FixedString64Bytes(validatedName);
    }
}
