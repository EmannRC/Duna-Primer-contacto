using UnityEngine;

namespace Duna.QuestSystem
{
    [System.Serializable]
    public class QuestObjectiveData
    {
        [Header("General")]

        [SerializeField] private ObjectiveType objectiveType;

        [SerializeField] private string targetID;

        [SerializeField] private int requiredAmount = 1;

        [TextArea]
        [SerializeField] private string description;

        public ObjectiveType ObjectiveType => objectiveType;
        public string TargetID => targetID;
        public int RequiredAmount => requiredAmount;
        public string Description => description;
    }
}
