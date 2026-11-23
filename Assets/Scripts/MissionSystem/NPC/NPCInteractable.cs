using Duna.InteractionSystem;
using Unity.Netcode;
using UnityEngine;

namespace Duna.Interaction
{
    public class NPCInteractable : MonoBehaviour
    {
        [Header("UI Sobre el NPC")]
        [SerializeField] private GameObject promptPanel;

        private IInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<IInteractable>();

            if (interactable == null)
                Debug.LogError($"{gameObject.name} necesita un componente que implemente IInteractable.");

            if (promptPanel != null)
                promptPanel.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!TryGetLocalPlayer(other, out PlayerContext playerContext))
                return;

            if (interactable != null && playerContext.interaction != null)
                playerContext.interaction.SetInteractable(interactable);

            if (promptPanel != null)
                promptPanel.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!TryGetLocalPlayer(other, out PlayerContext playerContext))
                return;

            if (interactable != null && playerContext.interaction != null)
                playerContext.interaction.ClearInteractable(interactable);

            if (promptPanel != null)
                promptPanel.SetActive(false);
        }

        private bool TryGetLocalPlayer(Collider other, out PlayerContext playerContext)
        {
            playerContext = null;

            if (!other.CompareTag("Player"))
                return false;

            playerContext = other.GetComponentInParent<PlayerContext>();
            if (playerContext == null)
                return false;

            NetworkObject netObj = playerContext.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsOwner)
                return false;

            return true;
        }

    }
}
