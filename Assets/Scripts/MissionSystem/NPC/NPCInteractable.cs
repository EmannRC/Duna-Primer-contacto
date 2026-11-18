using Duna.InteractionSystem;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Duna.Interaction
{
    public class NPCInteractable : MonoBehaviour
    {
        [Header("UI Sobre el NPC")]
        [SerializeField] private GameObject promptPanel;

        [Header("Configuración")]
        [SerializeField] private bool faceCamera = true;

        private IInteractable interactable;
        private Camera localCamera;

        private void Awake()
        {
            interactable = GetComponent<IInteractable>();

            if (interactable == null)
            {
                Debug.LogError(
                    $"{gameObject.name} necesita un componente que implemente IInteractable."
                );
            }

            if (promptPanel != null)
                promptPanel.SetActive(false);

        }

        private void LateUpdate()
        {
            // Hacer que el panel mire siempre hacia la cámara del jugador local
            if (!faceCamera || promptPanel == null || !promptPanel.activeSelf)
                return;

            if (localCamera == null)
            {
                if (LocalPlayerBootstrap.Instance != null &&
                    LocalPlayerBootstrap.Instance.LocalPlayer != null)
                {
                    localCamera = LocalPlayerBootstrap.Instance.LocalPlayer.mainCamera;
                }
                else
                {
                    localCamera = Camera.main;
                }
            }

            if (localCamera != null)
            {
                promptPanel.transform.rotation = localCamera.transform.rotation;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!TryGetLocalPlayer(other, out PlayerContext playerContext))
                return;

            if (interactable != null && playerContext.interaction != null)
            {
                playerContext.interaction.SetInteractable(interactable);
            }

            promptPanel.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!TryGetLocalPlayer(other, out PlayerContext playerContext))
                return;

            if (interactable != null && playerContext.interaction != null)
            {
                playerContext.interaction.ClearInteractable(interactable);
            }

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

            // Verificar que sea el jugador local en multijugador
            NetworkObject netObj = playerContext.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsOwner)
                return false;

            return true;
        }

        
    }
}
