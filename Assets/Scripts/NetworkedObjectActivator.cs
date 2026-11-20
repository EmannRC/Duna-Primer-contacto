using Unity.Netcode;
using UnityEngine;

public class NetworkedObjectActivator : NetworkBehaviour
{
    [Header("Hijos visuales y colisionadores del cristal")]
    [SerializeField] private GameObject crystalVisualsAndCollider;

    private readonly NetworkVariable<bool> isActivated = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        ApplyState(isActivated.Value);
        isActivated.OnValueChanged += OnStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        isActivated.OnValueChanged -= OnStateChanged;
    }

    private void OnStateChanged(bool previousValue, bool newValue)
    {
        ApplyState(newValue);
    }

    private void ApplyState(bool active)
    {
        if (crystalVisualsAndCollider != null)
            crystalVisualsAndCollider.SetActive(active);
    }

    // Método público para conectar en el UnityEvent del DialogueController
    public void ActivateCrystals()
    {
        if (IsServer)
        {
            isActivated.Value = true;
        }
        else
        {
            RequestActivateServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestActivateServerRpc()
    {
        isActivated.Value = true;
    }
}

