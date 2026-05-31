using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworkSetup : NetworkBehaviour
{
    [SerializeField] private UnityEngine.InputSystem.PlayerInput playerInput;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            playerInput.enabled = false;

            return;
        }
        InitializeLocalPlayer();

    }
    private void InitializeLocalPlayer()
    {
        LocalPlayerBootstrap.Instance.Setup(transform.root);
    }
}
