using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    [CreateAssetMenu(
        fileName = "Quest",
        menuName = "Duna/Quest System/Quest")]
    public class QuestData : ScriptableObject
    {
        [Header("Info")]

        [SerializeField] private string questID;

        [SerializeField] private string questName;

        [TextArea(4, 6)]
        [SerializeField] private string description;

        [Header("Objectives")]

        [SerializeField]
        private List<QuestObjectiveData> objectives = new();

        [Header("Rewards")]

        [SerializeField]
        private List<RewardData> rewards = new();

        [Header("Settings")]

        [SerializeField]
        private bool repeatable;

        [SerializeField]
        private bool autoComplete;

        public string QuestID => questID;
        public string QuestName => questName;
        public string Description => description;

        public IReadOnlyList<QuestObjectiveData> Objectives => objectives;
        public IReadOnlyList<RewardData> Rewards => rewards;

        public bool Repeatable => repeatable;
        public bool AutoComplete => autoComplete;
    }
}
