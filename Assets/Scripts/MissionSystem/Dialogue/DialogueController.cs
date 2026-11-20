using Duna.InteractionSystem;
using Duna.QuestSystem;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Duna.DialogueSystem
{
    public class DialogueController : MonoBehaviour, IInteractable
    {
        [Header("Dialogue - Default")]
        [SerializeField] private DialogueData defaultDialogue;

        [SerializeField] private bool shareDialogueRewards = true;

        [Header("Dialogue Events")]
        [SerializeField] private UnityEvent onQuestDialogueFinished;
        [SerializeField] private UnityEvent onInProgressDialogueFinished;
        [SerializeField] private UnityEvent onCompletionDialogueFinished;

        private PlayerContext currentPlayer;
        private QuestData currentQuest;
        private QuestObjectiveData currentObjective;
        private DialogueState currentDialogueState;

        private enum DialogueState { None, Offer, Objective, Progress, Default }


        //================================================//
        // HELPERS DE RESOLUCIÓN
        //================================================//

        private QuestManager ResolveQuestManager(PlayerContext player)
        {
            if (player != null && player.questManager != null)
                return player.questManager;

            return QuestManager.Instance;
        }

        //================================================//
        // INTERACTION
        //================================================//

        public void Interact(GameObject player)
        {
            if (player == null || DialogueManager.Instance == null)
                return;

            PlayerContext interactingPlayer = player.GetComponent<PlayerContext>();
            if (interactingPlayer == null || interactingPlayer.state == null)
                return;

            // En multijugador, asegurarnos de que solo el jugador local abra su propia UI de diálogo
            NetworkObject netObj = interactingPlayer.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsOwner)
                return;

            QuestManager manager = ResolveQuestManager(interactingPlayer);
            if (manager == null)
                return;

            currentPlayer = interactingPlayer;

            QuestGiver giver = GetComponent<QuestGiver>();
            QuestReceiver receiver = GetComponent<QuestReceiver>();
            NPCIdentity identity = GetComponent<NPCIdentity>();

            QuestData quest = giver != null
                ? giver.Quest
                : receiver != null ? receiver.Quest : null;

            QuestInstance active = quest != null
                ? manager.GetActiveQuest(quest.QuestID)
                : null;

            if (active != null && active.CurrentObjective != null)
            {
                QuestObjectiveData objective = active.CurrentObjective.Data;

                bool talkHere =
                    identity != null &&
                    objective.ObjectiveType == ObjectiveType.Talk &&
                    objective.TargetID == identity.NPCID;

                bool deliverHere =
                    objective.ObjectiveType == ObjectiveType.Deliver &&
                    receiver != null &&
                    receiver.CanDeliver(manager);

                if (talkHere || deliverHere)
                {
                    StartObjectiveDialogue(quest, objective);
                    return;
                }

                StartInProgressDialogue(quest);
                return;
            }

            if (active == null &&
                giver != null &&
                giver.Quest != null &&
                !manager.IsQuestCompleted(giver.Quest.QuestID))
            {
                quest = giver.Quest;

                QuestObjectiveData first =
                    quest.Objectives.Count > 0 ? quest.Objectives[0] : null;

                bool firstTalkHere =
                    identity != null &&
                    first != null &&
                    first.ObjectiveType == ObjectiveType.Talk &&
                    first.TargetID == identity.NPCID;

                if (firstTalkHere)
                {
                    StartObjectiveDialogue(quest, first);
                    return;
                }

                StartDialogue(
                    giver.OfferDialogue != null
                        ? giver.OfferDialogue
                        : defaultDialogue,
                    quest,
                    null,
                    DialogueState.Offer
                );

                return;
            }

            StartDialogue(
                defaultDialogue,
                null,
                null,
                DialogueState.Default
            );
        }

        //================================================//
        // OBJECTIVE DIALOGUE
        //================================================//

        private void StartObjectiveDialogue(
            QuestData quest,
            QuestObjectiveData objective)
        {
            DialogueData dialogue =
                objective.Dialogue != null
                    ? objective.Dialogue
                    : defaultDialogue;

            if (dialogue == null)
            {
                Debug.LogWarning(
                    $"[{name}] El objetivo no tiene diálogo y el NPC tampoco tiene uno genérico."
                );
                return;
            }

            currentQuest = quest;
            currentObjective = objective;
            currentDialogueState = DialogueState.Objective;

            DialogueManager.Instance.StartDialogue(
                dialogue,
                this,
                currentPlayer
            );
        }

        //================================================//
        // IN PROGRESS DIALOGUE
        //================================================//

        private void StartInProgressDialogue(QuestData quest)
        {
            DialogueData dialogue =
                quest != null ? quest.InProgressDialogue : null;

            if (dialogue == null)
            {
                StartDialogue(
                    defaultDialogue,
                    null,
                    null,
                    DialogueState.Default
                );
                return;
            }

            currentQuest = quest;
            currentObjective = null;
            currentDialogueState = DialogueState.Progress;

            DialogueManager.Instance.StartDialogue(
                dialogue,
                this,
                currentPlayer
            );
        }

        //================================================//
        // GENERAL DIALOGUE
        //================================================//

        private void StartDialogue(
            DialogueData dialogue,
            QuestData quest,
            QuestObjectiveData objective,
            DialogueState state)
        {
            if (dialogue == null)
            {
                if (state == DialogueState.Offer && quest != null)
                {
                    QuestManager manager = ResolveQuestManager(currentPlayer);
                    if (manager != null)
                        manager.RequestAcceptQuest(quest.QuestID);
                }

                ResetDialogueState();
                return;
            }

            currentQuest = quest;
            currentObjective = objective;
            currentDialogueState = state;

            DialogueManager.Instance.StartDialogue(
                dialogue,
                this,
                currentPlayer
            );
        }

        //================================================//
        // DIALOGUE FINISHED
        //================================================//

        public void DialogueFinished()
        {
            QuestManager manager = ResolveQuestManager(currentPlayer);

            if (currentPlayer == null || manager == null)
            {
                ResetDialogueState();
                return;
            }

            QuestData quest = currentQuest;
            QuestObjectiveData objective = currentObjective;
            DialogueState state = currentDialogueState;

            QuestGiver giver = GetComponent<QuestGiver>();
            QuestReceiver receiver = GetComponent<QuestReceiver>();
            PlayerContext player = currentPlayer;

            if (state == DialogueState.Offer)
            {
                if (giver != null && giver.HasQuest())
                {
                    // Verificamos antes si ya está activa o completada para no duplicar recompensas
                    bool canRequest = !manager.HasQuest(giver.Quest.QuestID) &&
                                      (giver.Quest.Repeatable || !manager.IsQuestCompleted(giver.Quest.QuestID));

                    if (canRequest)
                    {
                        manager.RequestAcceptQuest(giver.Quest.QuestID);
                        GiveReward(giver.OfferReward, player);
                    }
                }

                onQuestDialogueFinished?.Invoke();
            }
            else if (state == DialogueState.Progress)
            {
                RaiseTalkEvent();

                if (quest != null)
                    GiveReward(quest.InProgressDialogueData?.reward, player);

                onInProgressDialogueFinished?.Invoke();
            }
            else if (state == DialogueState.Objective &&
                     quest != null &&
                     objective != null)
            {
                QuestInstance active = manager.GetActiveQuest(quest.QuestID);
                bool newlyRequested = false;

                if (active == null && giver != null && giver.Quest == quest)
                {
                    if (quest.Repeatable || !manager.IsQuestCompleted(quest.QuestID))
                    {
                        manager.RequestAcceptQuest(quest.QuestID);
                        newlyRequested = true;
                    }
                }

                if (objective.ObjectiveType == ObjectiveType.Deliver &&
                    active != null &&
                    receiver != null &&
                    receiver.CanDeliver(manager))
                {
                    QuestItemDelivery delivery = GetComponent<QuestItemDelivery>();

                    if (delivery != null)
                    {
                        QuestObjectiveRuntime runtimeObjective = active.CurrentObjective;
                        ResetDialogueState();

                        delivery.Deliver(
                            manager,
                            player.inventory,
                            delivered => HandleDeliveryCompleted(
                                manager,
                                quest,
                                objective,
                                runtimeObjective,
                                player,
                                delivered
                            )
                        );

                        return;
                    }

                    Debug.LogError($"[{name}] Falta QuestItemDelivery.");
                }
                else if (objective.ObjectiveType == ObjectiveType.Talk)
                {
                    if (active != null)
                    {
                        ProcessTalkObjectiveAsync(manager, quest, objective, player);
                    }
                    else if (newlyRequested)
                    {
                        // Si la misión se acaba de pedir como cliente, esperamos el RPC de aceptación del servidor
                        StartCoroutine(WaitAcceptThenTalkRoutine(manager, quest, objective, player));
                    }
                }

                onCompletionDialogueFinished?.Invoke();
                onInProgressDialogueFinished?.Invoke();
            }

            ResetDialogueState();
        }

        //================================================//
        // ASYNC NETWORK ROUTINES (Para Clientes y Host)
        //================================================//

        private IEnumerator WaitAcceptThenTalkRoutine(
            QuestManager manager,
            QuestData quest,
            QuestObjectiveData objective,
            PlayerContext player)
        {
            float timeout = 2.5f;
            while (manager.GetActiveQuest(quest.QuestID) == null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (manager.GetActiveQuest(quest.QuestID) != null)
            {
                ProcessTalkObjectiveAsync(manager, quest, objective, player);
            }
        }

        private void ProcessTalkObjectiveAsync(
            QuestManager manager,
            QuestData quest,
            QuestObjectiveData objective,
            PlayerContext player)
        {
            QuestInstance current = manager.GetActiveQuest(quest.QuestID);
            if (current == null || current.CurrentObjective == null)
                return;

            int targetObjIndex = current.CurrentObjectiveIndex;
            bool isLastObjective = targetObjIndex >= quest.Objectives.Count - 1;

            // Envía el progreso Talk al servidor (vía QuestEvents -> QuestTracker -> ServerRpc)
            RaiseTalkEvent();

            // Esperamos confirmación del servidor de que el objetivo avanzó o la misión se completó
            StartCoroutine(WaitTalkConfirmationRoutine(
                manager,
                quest,
                objective,
                player,
                targetObjIndex,
                isLastObjective
            ));
        }

        private IEnumerator WaitTalkConfirmationRoutine(
            QuestManager manager,
            QuestData quest,
            QuestObjectiveData objective,
            PlayerContext player,
            int previousObjIndex,
            bool isLastObjective)
        {
            float timeout = 2.5f;
            while (timeout > 0f)
            {
                QuestInstance q = manager.GetActiveQuest(quest.QuestID);

                // Si la misión se autocompletó y desapareció de activeQuests, o cambió de estado/índice:
                if (q == null || q.IsCompleted || q.CurrentObjectiveIndex > previousObjIndex)
                {
                    GiveReward(objective.Reward, player);

                    if (q != null && q.IsCompleted && !quest.AutoComplete)
                    {
                        manager.RequestTurnInQuest(quest.QuestID);
                    }
                    yield break;
                }

                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        //================================================//
        // HELPERS
        //================================================//

        private void RaiseTalkEvent()
        {
            NPCIdentity identity = GetComponent<NPCIdentity>();

            if (identity != null &&
                !string.IsNullOrWhiteSpace(identity.NPCID))
            {
                QuestEvents.RaiseTalkToNPC(identity.NPCID);
            }
        }

        private void GiveReward(
            QuestData.DialogueReward reward,
            PlayerContext player)
        {
            if (reward == null ||
                !reward.giveReward ||
                string.IsNullOrWhiteSpace(reward.itemID) ||
                reward.amount <= 0)
                return;

            if (shareDialogueRewards && QuestManager.Instance != null)
            {
                QuestManager.Instance.RequestSharedDialogueReward(reward.itemID, reward.amount);
            }
            else if (player != null)
            {
                Inventory inventory = player.GetComponentInChildren<Inventory>();
                if (inventory != null)
                    inventory.RequestAddItem(reward.itemID, reward.amount);
            }
        }

        private void HandleDeliveryCompleted(
            QuestManager manager,
            QuestData quest,
            QuestObjectiveData objective,
            QuestObjectiveRuntime runtimeObjective,
            PlayerContext player,
            bool delivered)
        {
            if (delivered &&
                runtimeObjective != null &&
                runtimeObjective.IsCompleted)
            {
                GiveReward(objective.Reward, player);

                // Esperar a que el servidor sincronice que la misión está completada antes de pedir TurnIn
                if (!quest.AutoComplete)
                {
                    StartCoroutine(WaitDeliveryTurnInRoutine(manager, quest.QuestID));
                }
            }
            else
            {
                Debug.LogWarning($"[{name}] No se pudo realizar la entrega.");
            }

            onCompletionDialogueFinished?.Invoke();
        }

        private IEnumerator WaitDeliveryTurnInRoutine(QuestManager manager, string questID)
        {
            float timeout = 2.5f;
            while (timeout > 0f)
            {
                QuestInstance active = manager.GetActiveQuest(questID);
                if (active == null) yield break; // Ya fue entregada

                if (active.IsCompleted)
                {
                    manager.RequestTurnInQuest(questID);
                    yield break;
                }

                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        private void ResetDialogueState()
        {
            currentDialogueState = DialogueState.None;
            currentQuest = null;
            currentObjective = null;
            currentPlayer = null;
        }


    }
}
