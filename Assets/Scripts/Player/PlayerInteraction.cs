using Duna.DialogueSystem;
using UnityEngine;

namespace Duna.InteractionSystem
{
    public class PlayerInteraction : MonoBehaviour
    {
        private PlayerContext ctx;

        private IInteractable currentInteractable;


        //================================================//
        // BIND
        //================================================//

        public void Bind(
            PlayerContext playerContext
        )
        {
            ctx =
                playerContext;


            if (ctx == null)
            {
                Debug.LogError(
                    "PlayerInteraction recibió PlayerContext NULL."
                );

                return;
            }


            ctx.inputEvents.InteractPressed
                += HandleInteract;
        }


        //================================================//
        // DESTROY
        //================================================//

        private void OnDestroy()
        {
            if (ctx == null)
                return;


            ctx.inputEvents.InteractPressed
                -= HandleInteract;
        }


        //================================================//
        // INTERACT
        //================================================//

        private void HandleInteract()
        {
            if (ctx == null)
                return;


            if (ctx.health.IsDead.Value)
                return;


            //============================================//
            // SI HAY DIÁLOGO ACTIVO
            //============================================//

            if (
                DialogueManager.Instance != null &&
                DialogueManager.Instance.IsDialogueActive
            )
            {
                DialogueManager.Instance
                    .AdvanceDialogue();


                return;
            }


            //============================================//
            // INTERACCIÓN NORMAL
            //============================================//

            if (currentInteractable == null)
            {
                Debug.Log(
                    "No hay nada interactuable cerca."
                );


                return;
            }


            currentInteractable.Interact(
                gameObject
            );
        }


        //================================================//
        // SET INTERACTABLE
        //================================================//

        public void SetInteractable(
            IInteractable interactable
        )
        {
            if (interactable == null || currentInteractable == interactable)
                return;

            currentInteractable =
                interactable;


            Debug.Log(
                "Interactuable detectado."
            );
        }


        //================================================//
        // CLEAR INTERACTABLE
        //================================================//

        public void ClearInteractable(
            IInteractable interactable
        )
        {
            if (
                currentInteractable ==
                interactable
            )
            {
                currentInteractable =
                    null;


                Debug.Log(
                    "Interactuable abandonado."
                );
            }
        }
    }
}
