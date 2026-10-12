using Duna.DialogueSystem;
using UnityEngine;

namespace Duna.QuestSystem
{
    public class QuestGiver : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private QuestData quest;
        [SerializeField] private DialogueData offerDialogue;
        [SerializeField] private QuestData.DialogueReward offerReward;

        public QuestData Quest => quest;
        public DialogueData OfferDialogue => offerDialogue;
        public QuestData.DialogueReward OfferReward => offerReward;

        public bool HasQuest() => quest != null;

        public bool GiveQuest(QuestManager questManager)
        {
            if (quest == null || questManager == null)
                return false;

            return questManager.AcceptQuest(quest.QuestID);
        }

        public void Interact(QuestManager questManager)
        {
            if (HasQuest())
                GiveQuest(questManager);
        }
    }
}
