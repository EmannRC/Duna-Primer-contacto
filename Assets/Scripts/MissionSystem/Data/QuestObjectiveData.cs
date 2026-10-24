using UnityEngine;
using Duna.DialogueSystem;

namespace Duna.QuestSystem
{
    [System.Serializable]
    public class QuestObjectiveData
    {
        //================================================//
        // GENERAL
        //================================================//

        [Header("General")]

        [SerializeField]
        private ObjectiveType objectiveType;

        [SerializeField]
        private string targetID;

        [SerializeField]
        private int requiredAmount = 1;

        [TextArea]
        [SerializeField]
        private string description;

        [Header("Talk Dialogue and Reward")]
        [SerializeField]
        private DialogueData dialogue;

        [SerializeField]
        private QuestData.DialogueReward reward;

        //================================================//
        // GETTERS
        //================================================//

        public ObjectiveType ObjectiveType => objectiveType;

        public string TargetID => targetID;

        public int RequiredAmount => requiredAmount;

        public string Description => description;

        public DialogueData Dialogue => dialogue;

        public QuestData.DialogueReward Reward => reward;

    }
}

