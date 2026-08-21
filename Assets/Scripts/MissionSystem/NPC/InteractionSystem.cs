using UnityEngine;

namespace Duna.InteractionSystem
{
    public class NPCInteractable : MonoBehaviour
    {
        private IInteractable interactable;

        private void Awake()
        {
            interactable =
                GetComponent<IInteractable>();

            if (interactable == null)
            {
                Debug.LogError(
                    gameObject.name +
                    " no tiene ningún IInteractable."
                );
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            SetPlayerInteractable(other);
        }

        private void OnTriggerStay(Collider other)
        {
            // También cubre jugadores que aparecen ya dentro del área
            // de interacción (por ejemplo, después de un respawn).
            SetPlayerInteractable(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerContext playerContext =
                other.GetComponent<PlayerContext>();

            if (playerContext == null || playerContext.interaction == null)
                return;

            playerContext.interaction.ClearInteractable(
                interactable
            );
        }

        private void SetPlayerInteractable(Collider other)
        {
            if (interactable == null || !other.CompareTag("Player"))
                return;

            // El CharacterController está en la raíz del jugador, mientras
            // que PlayerInteraction vive en su hijo "Interact". PlayerContext
            // mantiene la referencia válida entre ambos objetos.
            PlayerContext playerContext =
                other.GetComponentInParent<PlayerContext>();

            if (playerContext == null || playerContext.interaction == null)
                return;

            playerContext.interaction.SetInteractable(interactable);
        }
    }
}
