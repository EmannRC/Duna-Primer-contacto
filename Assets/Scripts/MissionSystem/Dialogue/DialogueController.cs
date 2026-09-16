using Duna.InteractionSystem;
using Duna.QuestSystem;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Duna.DialogueSystem
{
    public class DialogueController : MonoBehaviour, IInteractable
    {
        //================================================//
        // DEFAULT DIALOGUE
        //================================================//

        [Header("Dialogue - Default")]

        [SerializeField]
        private DialogueData defaultDialogue;


        //================================================//
        // DIALOGUE EVENTS
        //================================================//

        [Header("Dialogue Events")]

        [SerializeField]
        private UnityEvent onQuestDialogueFinished;

        [SerializeField]
        private UnityEvent onInProgressDialogueFinished;

        [SerializeField]
        private UnityEvent onCompletionDialogueFinished;


        //================================================//
        // CURRENT PLAYER
        //================================================//

        private PlayerContext currentPlayer;


        //================================================//
        // CURRENT QUEST
        //================================================//

        private QuestData currentQuest;


        //================================================//
        // DIALOGUE STATE
        //================================================//

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

        public void Interact(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("Player inválido.");
                return;
            }

            currentPlayer = player.GetComponent<PlayerContext>();

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

            QuestManager questManager = currentPlayer.questManager;

            if (questManager == null)
            {
                Debug.LogError(
                    "El jugador no tiene QuestManager."
                );

                return;
            }

            QuestGiver questGiver = GetComponent<QuestGiver>();
            QuestReceiver questReceiver = GetComponent<QuestReceiver>();


            //================================================//
            // NPC CON QUEST
            //================================================//

            if (questGiver != null &&
                questGiver.Quest != null)
            {
                QuestData quest = questGiver.Quest;

                string questID = quest.QuestID;

                QuestInstance activeQuest =
                    questManager.GetActiveQuest(questID);


                //================================================//
                // QUEST COMPLETABLE
                //================================================//

                if (activeQuest != null)
                {
                    // La misión ya terminó todos sus objetivos.
                    if (activeQuest.IsCompleted)
                    {
                        StartCompletionDialogue(quest);
                        return;
                    }

                    // La misión tiene un objetivo Deliver pendiente.
                    if (questReceiver != null &&
                        questReceiver.CanDeliver(questManager))
                    {
                        StartCompletionDialogue(quest);
                        return;
                    }

                    NPCIdentity identity = GetComponent<NPCIdentity>();

                    if (identity != null)
                    {
                        if (activeQuest.IsCurrentObjectiveTalkTo(identity.NPCID))
                        {
                            StartQuestDialogue(quest);
                            return;
                        }
                    }

                    StartInProgressDialogue(quest);
                    return;
                }


                //================================================//
                // QUEST ACTIVA
                //================================================//

                if (activeQuest != null)
                {
                    NPCIdentity identity =
                        GetComponent<NPCIdentity>();

                    if (identity != null)
                    {
                        if (activeQuest.IsCurrentObjectiveTalkTo(
                            identity.NPCID))
                        {
                            StartQuestDialogue(quest);

                            return;
                        }
                    }

                    StartInProgressDialogue(quest);

                    return;
                }


                //================================================//
                // QUEST NO ACEPTADA
                //================================================//

                if (!questManager.IsQuestCompleted(questID))
                {
                    StartQuestDialogue(quest);

                    return;
                }
            }


            //================================================//
            // NPC SIN QUEST O QUEST YA COMPLETADA
            //================================================//

            if (defaultDialogue != null)
            {
                StartDefaultDialogue();

                return;
            }

            Debug.LogWarning(
                $"El NPC {name} no tiene ningún diálogo asignado."
            );
        }


        //================================================//
        // START QUEST DIALOGUE
        //================================================//

        private void StartQuestDialogue(QuestData quest)
        {
            if (quest == null)
                return;

            if (quest.StartDialogue == null)
            {
                Debug.LogWarning(
                    $"La quest {quest.QuestID} no tiene Start Dialogue."
                );

                return;
            }

            currentQuest = quest;
            currentDialogueState = DialogueState.Quest;

            DialogueManager.Instance.StartDialogue(
                quest.StartDialogue,
                this
            );
        }


        //================================================//
        // START IN PROGRESS DIALOGUE
        //================================================//

        private void StartInProgressDialogue(QuestData quest)
        {
            if (quest == null)
                return;

            if (quest.InProgressDialogue == null)
            {
                Debug.LogWarning(
                    $"La quest {quest.QuestID} no tiene In Progress Dialogue."
                );

                return;
            }

            currentQuest = quest;
            currentDialogueState = DialogueState.InProgress;

            DialogueManager.Instance.StartDialogue(
                quest.InProgressDialogue,
                this
            );
        }


        //================================================//
        // START COMPLETION DIALOGUE
        //================================================//

        private void StartCompletionDialogue(QuestData quest)
        {
            if (quest == null)
                return;

            if (quest.CompletionDialogue == null)
            {
                Debug.LogWarning(
                    $"La quest {quest.QuestID} no tiene Completion Dialogue."
                );

                return;
            }

            currentQuest = quest;
            currentDialogueState = DialogueState.Completion;

            DialogueManager.Instance.StartDialogue(
                quest.CompletionDialogue,
                this
            );
        }


        //================================================//
        // START DEFAULT DIALOGUE
        //================================================//

        private void StartDefaultDialogue()
        {
            if (defaultDialogue == null)
                return;

            currentQuest = null;
            currentDialogueState = DialogueState.None;

            DialogueManager.Instance.StartDialogue(
                defaultDialogue,
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
                Debug.LogWarning(
                    $"[{name}] No hay jugador asociado al diálogo."
                );

                ResetDialogueState();

                return;
            }

            QuestManager questManager =
                currentPlayer.questManager;

            if (questManager == null)
            {
                ResetDialogueState();

                return;
            }


            //================================================//
            // DIÁLOGO FINAL
            //================================================//

            if (currentDialogueState == DialogueState.Completion)
            {
                QuestReceiver questReceiver =
                    GetComponent<QuestReceiver>();

                QuestInstance activeQuest = null;

                if (currentQuest != null)
                {
                    activeQuest =
                        questManager.GetActiveQuest(
                            currentQuest.QuestID
                        );
                }

                if (activeQuest == null)
                {
                    Debug.LogWarning(
                        $"[{name}] No se encontró la misión activa al finalizar el diálogo."
                    );

                    onCompletionDialogueFinished?.Invoke();
                    ResetDialogueState();
                    return;
                }

                //================================================//
                // CASO 1: LA MISIÓN YA ESTABA COMPLETADA
                //================================================//

                if (activeQuest.IsCompleted)
                {
                    bool turnedIn =
                        questManager.TurnInQuest(
                            activeQuest.Data.QuestID
                        );

                    if (turnedIn && currentQuest != null)
                    {
                        GiveReward(
                            currentQuest
                                .CompletionDialogueData
                                .reward
                        );
                    }

                    onCompletionDialogueFinished?.Invoke();
                    ResetDialogueState();
                    return;
                }

                //================================================//
                // CASO 2: HAY UN OBJETIVO DELIVER
                //================================================//

                if (questReceiver != null &&
                    questReceiver.CanDeliver(questManager))
                {
                    bool delivered =
                        questReceiver.DeliverItem(
                            questManager
                        );

                    if (delivered)
                    {
                        // El evento de entrega acaba de ejecutarse.
                        // Volvemos a consultar el estado de la misión.
                        activeQuest =
                            questManager.GetActiveQuest(
                                currentQuest.QuestID
                            );

                        if (activeQuest != null &&
                            activeQuest.IsCompleted)
                        {
                            bool turnedIn =
                                questManager.TurnInQuest(
                                    activeQuest.Data.QuestID
                                );

                            if (turnedIn && currentQuest != null)
                            {
                                GiveReward(
                                    currentQuest
                                        .CompletionDialogueData
                                        .reward
                                );
                            }
                        }
                    }
                }

                onCompletionDialogueFinished?.Invoke();
                ResetDialogueState();
                return;
            }


            //================================================//
            // DIÁLOGO INICIAL
            //================================================//

            if (currentDialogueState ==
                DialogueState.Quest)
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

                if (currentQuest != null)
                {
                    GiveReward(
                        currentQuest
                            .StartDialogueData
                            .reward
                    );
                }

                onQuestDialogueFinished?.Invoke();

                ResetDialogueState();

                return;
            }


            //================================================//
            // DIÁLOGO INTERMEDIO
            //================================================//

            if (currentDialogueState ==
                DialogueState.InProgress)
            {
                RaiseTalkEvent();

                if (currentQuest != null)
                {
                    GiveReward(
                        currentQuest
                            .InProgressDialogueData
                            .reward
                    );
                }

                onInProgressDialogueFinished?.Invoke();

                ResetDialogueState();

                return;
            }


            //================================================//
            // DEFAULT
            //================================================//

            ResetDialogueState();
        }


        //================================================//
        // TALK TO NPC EVENT
        //================================================//

        private void RaiseTalkEvent()
        {
            NPCIdentity identity =
                GetComponent<NPCIdentity>();

            if (identity == null ||
                string.IsNullOrWhiteSpace(
                    identity.NPCID
                ))
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


        //================================================//
        // GIVE REWARD
        //================================================//

        private void GiveReward(
            QuestData.DialogueReward reward)
        {
            if (reward == null)
                return;

            if (!reward.giveReward)
                return;

            if (currentPlayer == null)
            {
                Debug.LogWarning(
                    $"[{name}] No hay jugador asociado."
                );

                return;
            }

            if (string.IsNullOrWhiteSpace(
                reward.itemID
            ))
            {
                Debug.LogWarning(
                    $"[{name}] No hay Item ID configurado."
                );

                return;
            }

            if (reward.amount <= 0)
            {
                Debug.LogWarning(
                    $"[{name}] La cantidad de recompensa debe ser mayor a 0."
                );

                return;
            }

            Inventory inventory =
                currentPlayer.GetComponentInChildren<Inventory>();

            if (inventory == null)
            {
                Debug.LogWarning(
                    $"[{name}] El jugador no tiene Inventory."
                );

                return;
            }

            inventory.RequestAddItem(
                reward.itemID,
                reward.amount
            );

            Debug.Log(
                $"[{name}] Recompensa solicitada: " +
                $"{reward.itemID} x{reward.amount}"
            );
        }


        //================================================//
        // RESET DIALOGUE STATE
        //================================================//

        private void ResetDialogueState()
        {
            currentDialogueState = DialogueState.None;
            currentQuest = null;
            currentPlayer = null;
        }
    }
}
