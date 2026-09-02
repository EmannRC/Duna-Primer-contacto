using Duna.InteractionSystem;
using Duna.QuestSystem;
using UnityEngine;

namespace Duna.DialogueSystem
{
    public class DialogueController :
        MonoBehaviour,
        IInteractable
    {
        [Header("Dialogue - Mission")]
        [Tooltip("Diálogo que se muestra cuando el jugador todavía no tiene la misión.")]
        [SerializeField]
        private DialogueData questDialogue;


        [Header("Dialogue - In Progress")]
        [Tooltip("Diálogo que se muestra mientras la misión está activa pero todavía no está completada.")]
        [SerializeField]
        private DialogueData inProgressDialogue;


        [Header("Dialogue - Completion")]
        [Tooltip("Diálogo que se muestra cuando la misión está completada y puede entregarse.")]
        [SerializeField]
        private DialogueData completionDialogue;


        private PlayerContext currentPlayer;


        // Indica qué tipo de diálogo se está mostrando.
        private DialogueState currentDialogueState;


        private enum DialogueState
        {
            None,
            Quest,
            InProgress,
            Completion
        }


        //================================================//
        // INTERACT
        //================================================//

        public void Interact(
            GameObject player
        )
        {
            if (player == null)
            {
                Debug.LogError(
                    "Player inválido."
                );

                return;
            }


            currentPlayer =
                player.GetComponent<PlayerContext>();


            if (currentPlayer == null)
            {
                Debug.LogError(
                    "El jugador no tiene PlayerContext."
                );

                return;
            }


            if (DialogueManager.Instance == null)
            {
                Debug.LogError(
                    "No existe DialogueManager."
                );

                return;
            }


            QuestManager questManager =
                currentPlayer.questManager;


            if (questManager == null)
            {
                Debug.LogError(
                    "El jugador no tiene QuestManager."
                );

                return;
            }


            QuestGiver questGiver =
                GetComponent<QuestGiver>();


            QuestReceiver questReceiver =
                GetComponent<QuestReceiver>();


            //================================================//
            // COMPROBAR ESTADO DE LA MISIÓN
            //================================================//

            if (
                questGiver != null &&
                questGiver.Quest != null
            )
            {
                string questID =
                    questGiver.Quest.QuestID;


                QuestInstance activeQuest =
                    questManager.GetActiveQuest(
                        questID
                    );


                //================================================//
                // 1. OBJETIVO DE ENTREGA → DIÁLOGO FINAL
                //================================================//

                if (
                    activeQuest != null &&
                    questReceiver != null &&
                    questReceiver.CanDeliver(questManager)
                )
                {
                    StartCompletionDialogue();

                    return;
                }


                //================================================//
                // 2. MISIÓN ACTIVA → DIÁLOGO INTERMEDIO
                //================================================//

                if (activeQuest != null)
                {
                    NPCIdentity identity =
                        GetComponent<NPCIdentity>();

                    if (identity != null)
                    {
                        // Si el objetivo actual es hablar con este NPC
                        // y todavía no está completado, mostrar diálogo inicial.
                        if (activeQuest.IsCurrentObjectiveTalkTo(identity.NPCID))
                        {
                            StartQuestDialogue();

                            return;
                        }
                    }

                    // Si ya habló con este NPC, mostrar diálogo de progreso.
                    StartInProgressDialogue();

                    return;
                }


                //================================================//
                // 3. MISIÓN NO ACEPTADA → DIÁLOGO INICIAL
                //================================================//

                if (!questManager.IsQuestCompleted(questID))
                {
                    StartQuestDialogue();

                    return;
                }
            }


            //================================================//
            // NPC SIN QUEST GIVER
            //================================================//

            if (questDialogue != null)
            {
                StartQuestDialogue();

                return;
            }


            Debug.LogWarning(
                $"El NPC {name} no tiene ningún diálogo asignado."
            );
        }


        //================================================//
        // START QUEST DIALOGUE
        //================================================//

        private void StartQuestDialogue()
        {
            if (questDialogue == null)
            {
                Debug.LogWarning(
                    $"El NPC {name} no tiene Quest Dialogue."
                );

                return;
            }


            currentDialogueState =
                DialogueState.Quest;


            DialogueManager.Instance.StartDialogue(
                questDialogue,
                this
            );
        }


        //================================================//
        // START IN PROGRESS DIALOGUE
        //================================================//

        private void StartInProgressDialogue()
        {
            if (inProgressDialogue == null)
            {
                Debug.LogWarning(
                    $"El NPC {name} no tiene In Progress Dialogue."
                );

                return;
            }


            currentDialogueState =
                DialogueState.InProgress;


            DialogueManager.Instance.StartDialogue(
                inProgressDialogue,
                this
            );
        }


        //================================================//
        // START COMPLETION DIALOGUE
        //================================================//

        private void StartCompletionDialogue()
        {
            if (completionDialogue == null)
            {
                Debug.LogWarning(
                    $"El NPC {name} no tiene Completion Dialogue."
                );

                return;
            }


            currentDialogueState =
                DialogueState.Completion;


            DialogueManager.Instance.StartDialogue(
                completionDialogue,
                this
            );
        }


        //================================================//
        // DIALOGUE FINISHED
        //================================================//

        public void DialogueFinished()
        {
            if (currentPlayer == null)
            {
                Debug.LogError(
                    "No existe jugador asociado al diálogo."
                );

                return;
            }


            QuestManager questManager =
                currentPlayer.questManager;


            if (questManager == null)
            {
                Debug.LogError(
                    "El jugador no tiene QuestManager."
                );

                return;
            }


            //================================================//
            // DIÁLOGO FINAL
            //================================================//

            if (
    currentDialogueState ==
    DialogueState.Completion
)
            {
                QuestReceiver questReceiver =
                    GetComponent<QuestReceiver>();

                if (questReceiver != null)
                {
                    // Primero completa el objetivo Deliver
                    bool delivered =
                        questReceiver.DeliverItem(
                            questManager
                        );

                    if (delivered)
                    {
                        // Ahora la misión debería estar Completed
                        questReceiver.TurnInQuest(
                            questManager
                        );
                    }
                }

                currentDialogueState =
                    DialogueState.None;

                return;
            }


            //================================================//
            // DIÁLOGO INICIAL
            //================================================//

            if (currentDialogueState == DialogueState.Quest)
            {
                QuestGiver questGiver =
                    GetComponent<QuestGiver>();

                if (questGiver != null)
                {
                    questGiver.GiveQuest(
                        questManager
                    );
                }

                RaiseTalkEvent();

                currentDialogueState =
                    DialogueState.None;

                return;
            }


            //================================================//
            // DIÁLOGO INTERMEDIO
            //================================================//

            if (
                currentDialogueState ==
                DialogueState.InProgress
            )
            {
                // No se acepta la misión otra vez.
                // No se entrega.
                // Simplemente cuenta como conversación.
                RaiseTalkEvent();


                currentDialogueState =
                    DialogueState.None;


                return;
            }


            currentDialogueState =
                DialogueState.None;
        }


        //================================================//
        // TALK TO NPC EVENT
        //================================================//

        private void RaiseTalkEvent()
        {
            NPCIdentity identity =
                GetComponent<NPCIdentity>();


            if (
                identity == null ||
                string.IsNullOrWhiteSpace(
                    identity.NPCID
                )
            )
            {
                Debug.LogWarning(
                    $"El NPC {name} no tiene un NPCIdentity válido."
                );

                return;
            }


            QuestEvents.RaiseTalkToNPC(
                identity.NPCID
            );
        }
    }
}
