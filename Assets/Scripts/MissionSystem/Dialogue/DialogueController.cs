using Duna.InteractionSystem;
using Duna.QuestSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Duna.DialogueSystem
{
    public class DialogueController : MonoBehaviour, IInteractable
    {
        [Header("Dialogue - Default")]
        [SerializeField] private DialogueData defaultDialogue;

        [Header("Dialogue Events")]
        [SerializeField] private UnityEvent onQuestDialogueFinished;
        [SerializeField] private UnityEvent onInProgressDialogueFinished;
        [SerializeField] private UnityEvent onCompletionDialogueFinished;

        private PlayerContext currentPlayer;
        private QuestData currentQuest;
        private QuestObjectiveData currentObjective;
        private DialogueState currentDialogueState;

        private enum DialogueState { None, Offer, Objective, Progress, Default }

        public void Interact(GameObject player)
        {
            if (player == null || DialogueManager.Instance == null)
                return;

            currentPlayer = player.GetComponent<PlayerContext>();
            if (currentPlayer == null || currentPlayer.questManager == null)
                return;

            QuestManager manager = currentPlayer.questManager;
            QuestGiver giver = GetComponent<QuestGiver>();
            QuestReceiver receiver = GetComponent<QuestReceiver>();
            NPCIdentity identity = GetComponent<NPCIdentity>();
            QuestData quest = giver != null ? giver.Quest : receiver != null ? receiver.Quest : null;
            QuestInstance active = quest != null ? manager.GetActiveQuest(quest.QuestID) : null;

            if (active != null && active.CurrentObjective != null)
            {
                QuestObjectiveData objective = active.CurrentObjective.Data;
                bool talkHere = identity != null && objective.ObjectiveType == ObjectiveType.Talk && objective.TargetID == identity.NPCID;
                bool deliverHere = objective.ObjectiveType == ObjectiveType.Deliver && receiver != null && receiver.CanDeliver(manager);
                if (talkHere || deliverHere)
                {
                    StartObjectiveDialogue(quest, objective);
                    return;
                }

                StartInProgressDialogue(quest);
                return;
            }

            if (active == null && giver != null && giver.Quest != null && !manager.IsQuestCompleted(giver.Quest.QuestID))
            {
                quest = giver.Quest;
                QuestObjectiveData first = quest.Objectives.Count > 0 ? quest.Objectives[0] : null;
                bool firstTalkHere = identity != null && first != null && first.ObjectiveType == ObjectiveType.Talk && first.TargetID == identity.NPCID;
                if (firstTalkHere)
                {
                    StartObjectiveDialogue(quest, first);
                    return;
                }

                StartDialogue(giver.OfferDialogue != null ? giver.OfferDialogue : defaultDialogue, quest, null, DialogueState.Offer);
                return;
            }

            StartDialogue(defaultDialogue, null, null, DialogueState.Default);
        }

        private void StartObjectiveDialogue(QuestData quest, QuestObjectiveData objective)
        {
            DialogueData dialogue = objective.Dialogue != null ? objective.Dialogue : defaultDialogue;
            if (dialogue == null)
            {
                Debug.LogWarning($"[{name}] El objetivo no tiene diálogo y el NPC tampoco tiene uno genérico.");
                return;
            }

            currentQuest = quest;
            currentObjective = objective;
            currentDialogueState = DialogueState.Objective;
            DialogueManager.Instance.StartDialogue(dialogue, this);
        }

        private void StartInProgressDialogue(QuestData quest)
        {
            DialogueData dialogue = quest != null ? quest.InProgressDialogue : null;
            if (dialogue == null)
            {
                StartDialogue(defaultDialogue, null, null, DialogueState.Default);
                return;
            }

            currentQuest = quest;
            currentObjective = null;
            currentDialogueState = DialogueState.Progress;
            DialogueManager.Instance.StartDialogue(dialogue, this);
        }

        private void StartDialogue(DialogueData dialogue, QuestData quest, QuestObjectiveData objective, DialogueState state)
        {
            if (dialogue == null)
            {
                if (state == DialogueState.Offer && quest != null)
                    currentPlayer.questManager.AcceptQuest(quest.QuestID);
                ResetDialogueState();
                return;
            }

            currentQuest = quest;
            currentObjective = objective;
            currentDialogueState = state;
            DialogueManager.Instance.StartDialogue(dialogue, this);
        }

        public void DialogueFinished()
        {
            if (currentPlayer == null || currentPlayer.questManager == null)
            {
                ResetDialogueState();
                return;
            }

            QuestManager manager = currentPlayer.questManager;
            QuestData quest = currentQuest;
            QuestObjectiveData objective = currentObjective;
            DialogueState state = currentDialogueState;
            QuestGiver giver = GetComponent<QuestGiver>();
            QuestReceiver receiver = GetComponent<QuestReceiver>();
            PlayerContext player = currentPlayer;

            if (state == DialogueState.Offer)
            {
                bool accepted = giver != null && giver.GiveQuest(manager);
                if (accepted && giver != null)
                    GiveReward(giver.OfferReward, player);
                onQuestDialogueFinished?.Invoke();
            }
            else if (state == DialogueState.Progress)
            {
                RaiseTalkEvent();
                if (quest != null)
                    GiveReward(quest.InProgressDialogueData?.reward, player);
                onInProgressDialogueFinished?.Invoke();
            }
            else if (state == DialogueState.Objective && quest != null && objective != null)
            {
                QuestInstance active = manager.GetActiveQuest(quest.QuestID);
                bool newlyAccepted = false;
                if (active == null && giver != null && giver.Quest == quest)
                    newlyAccepted = giver.GiveQuest(manager);

                if (objective.ObjectiveType == ObjectiveType.Deliver && active != null && receiver != null && receiver.CanDeliver(manager))
                {
                    QuestItemDelivery delivery = GetComponent<QuestItemDelivery>();
                    if (delivery != null)
                    {
                        QuestObjectiveRuntime runtimeObjective = active.CurrentObjective;
                        ResetDialogueState();
                        delivery.Deliver(manager, player.inventory, delivered => HandleDeliveryCompleted(manager, quest, objective, runtimeObjective, player, delivered));
                        return;
                    }
                    Debug.LogError($"[{name}] Falta QuestItemDelivery.");
                }
                else if (objective.ObjectiveType == ObjectiveType.Talk)
                {
                    if (active != null || newlyAccepted)
                    {
                        QuestInstance talkingQuest = active ?? manager.GetActiveQuest(quest.QuestID);
                        QuestObjectiveRuntime runtimeObjective = talkingQuest != null ? talkingQuest.CurrentObjective : null;
                        RaiseTalkEvent();
                        QuestInstance afterTalk = manager.GetActiveQuest(quest.QuestID);
                        if (afterTalk != null && afterTalk.IsCompleted && !quest.AutoComplete)
                            manager.TurnInQuest(quest.QuestID);
                        if (runtimeObjective != null && runtimeObjective.IsCompleted)
                            GiveReward(objective.Reward, player);
                    }
                }

                onCompletionDialogueFinished?.Invoke();
                onInProgressDialogueFinished?.Invoke();
            }

            ResetDialogueState();
        }

        private void RaiseTalkEvent()
        {
            NPCIdentity identity = GetComponent<NPCIdentity>();
            if (identity != null && !string.IsNullOrWhiteSpace(identity.NPCID))
                QuestEvents.RaiseTalkToNPC(identity.NPCID);
        }

        private void GiveReward(QuestData.DialogueReward reward, PlayerContext player)
        {
            if (reward == null || !reward.giveReward || player == null || string.IsNullOrWhiteSpace(reward.itemID) || reward.amount <= 0)
                return;

            Inventory inventory = player.GetComponentInChildren<Inventory>();
            if (inventory != null)
                inventory.RequestAddItem(reward.itemID, reward.amount);
        }

        private void HandleDeliveryCompleted(QuestManager manager, QuestData quest, QuestObjectiveData objective, QuestObjectiveRuntime runtimeObjective, PlayerContext player, bool delivered)
        {
            if (delivered && runtimeObjective != null && runtimeObjective.IsCompleted)
            {
                GiveReward(objective.Reward, player);
                QuestInstance active = manager.GetActiveQuest(quest.QuestID);
                if (active != null && active.IsCompleted && !quest.AutoComplete)
                    manager.TurnInQuest(quest.QuestID);
            }
            else
            {
                Debug.LogWarning($"[{name}] No se pudo realizar la entrega.");
            }

            onCompletionDialogueFinished?.Invoke();
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
