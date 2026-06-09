using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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

        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        while (LocalPlayerBootstrap.Instance == null)
        {
            yield return null;
        }

        LocalPlayerBootstrap.Instance.Setup(transform.root);
    }
}
