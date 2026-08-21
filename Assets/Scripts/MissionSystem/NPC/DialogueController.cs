using Duna.InteractionSystem;
using Duna.QuestSystem;
using UnityEngine;

namespace Duna.DialogueSystem
{
    public class DialogueController :
        MonoBehaviour,
        IInteractable
    {
        [Header("Dialogue")]

        [SerializeField]
        private DialogueData dialogueData;


        private PlayerContext currentPlayer;


        //================================================//
        // INTERACT
        //================================================//

        public void Interact(
            GameObject player
        )
        {
            if (
                player == null
            )
            {
                Debug.LogError(
                    "Player inválido."
                );


                return;
            }


            currentPlayer =
                player.GetComponent<
                    PlayerContext
                >();


            if (
                currentPlayer == null
            )
            {
                Debug.LogError(
                    "El jugador no tiene PlayerContext."
                );


                return;
            }


            if (
                dialogueData == null
            )
            {
                Debug.LogError(
                    $"El NPC {name} no tiene DialogueData."
                );


                return;
            }


            if (
                DialogueManager.Instance == null
            )
            {
                Debug.LogError(
                    "No existe DialogueManager."
                );


                return;
            }


            DialogueManager.Instance.StartDialogue(
                dialogueData,
                this
            );
        }


        //================================================//
        // DIALOGUE FINISHED
        //================================================//

        public void DialogueFinished()
        {
            if (
                currentPlayer == null
            )
            {
                Debug.LogError(
                    "No existe jugador asociado al diálogo."
                );


                return;
            }


            QuestManager questManager =
                currentPlayer.questManager;


            if (
                questManager == null
            )
            {
                Debug.LogError(
                    "El jugador no tiene QuestManager."
                );


                return;
            }


            QuestReceiver questReceiver = GetComponent<QuestReceiver>();

            if (questReceiver != null && questReceiver.CanTurnIn(questManager))
            {
                questReceiver.TurnInQuest(questManager);
                return;
            }

            QuestGiver questGiver = GetComponent<QuestGiver>();

            // La primera conversación puede ofrecer la misión. Las siguientes
            // conversaciones siguen contando para objetivos de tipo Talk.
            if (questGiver != null)
            {
                questGiver.GiveQuest(questManager);
            }

            NPCIdentity identity = GetComponent<NPCIdentity>();

            if (identity == null || string.IsNullOrWhiteSpace(identity.NPCID))
            {
                Debug.LogWarning($"El NPC {name} no tiene un NPCIdentity válido para las misiones.");
                return;
            }

            // Se emite después de aceptar la misión para que la conversación
            // que la inicia pueda completar un objetivo de hablar con este NPC.
            QuestEvents.RaiseTalkToNPC(identity.NPCID);
        }
    }
}
