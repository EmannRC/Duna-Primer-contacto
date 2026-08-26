using Duna.InteractionSystem;
using UnityEngine;

namespace Duna.Interaction
{
    public class NPCInteractable : MonoBehaviour
    {
        private IInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<IInteractable>();

            if (interactable == null)
            {
                Debug.LogError(
                    $"{gameObject.name} necesita un componente que implemente IInteractable."
                );
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TrySetInteractable(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            PlayerContext playerContext =
                other.GetComponentInParent<PlayerContext>();

            if (playerContext == null || playerContext.interaction == null)
                return;

            playerContext.interaction.ClearInteractable(interactable);
        }

        private void TrySetInteractable(Collider other)
        {
            if (interactable == null)
                return;

            if (!other.CompareTag("Player"))
                return;

            PlayerContext playerContext =
                other.GetComponentInParent<PlayerContext>();

            if (playerContext == null || playerContext.interaction == null)
                return;

            playerContext.interaction.SetInteractable(interactable);
        }
    }
}
