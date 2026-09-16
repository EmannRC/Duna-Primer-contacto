using Unity.Netcode;
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        NetworkObject networkObject = other.GetComponentInParent<NetworkObject>();

        if (networkObject == null)
            return;

        if (!networkObject.IsPlayerObject)
            return;

        CharacterController controller =
            networkObject.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        networkObject.transform.position = destination.position;
        networkObject.transform.rotation = destination.rotation;

        if (controller != null)
            controller.enabled = true;
    }

}
