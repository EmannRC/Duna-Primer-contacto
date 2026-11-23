using Duna.DialogueSystem;
using UnityEngine;

namespace Duna.InteractionSystem
{
    public interface IInteractable
    {
        void Interact(GameObject player);
    }

    public class PlayerInteraction : MonoBehaviour
    {
        private PlayerContext ctx;
        private IInteractable currentInteractable;

        public void Bind(PlayerContext playerContext)
        {
            ctx = playerContext;

            if (ctx == null)
            {
                Debug.LogError("PlayerInteraction recibió PlayerContext NULL.");
                return;
            }

            ctx.inputEvents.InteractPressed += HandleInteract;
        }

        private void OnDestroy()
        {
            if (ctx != null)
                ctx.inputEvents.InteractPressed -= HandleInteract;
        }

        private void HandleInteract()
        {
            if (ctx == null || ctx.health.IsDead.Value)
                return;

            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            {
                DialogueManager.Instance.AdvanceDialogue();
                return;
            }

            if (currentInteractable == null)
                return;

            currentInteractable.Interact(gameObject);
        }

        public void SetInteractable(IInteractable interactable)
        {
            if (interactable == null || currentInteractable == interactable)
                return;

            currentInteractable = interactable;
        }

        public void ClearInteractable(IInteractable interactable)
        {
            if (currentInteractable == interactable)
                currentInteractable = null;
        }
    }
}