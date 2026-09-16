using Duna.DialogueSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Duna.QuestSystem.QuestData;

namespace Duna.QuestSystem
{
    [CreateAssetMenu(
        fileName = "Quest",
        menuName = "Duna/Quest System/Quest")]
    public class QuestData : ScriptableObject
    {

        public enum QuestType
        {
            Main,
            Side
        }

        [SerializeField]
        private QuestType questType;

        //================================================//
        // DIALOGUE REWARD
        //================================================//

        [Serializable]
        public class DialogueReward
        {
            public bool giveReward;

            public string itemID;

            public int amount = 1;
        }


        //================================================//
        // QUEST DIALOGUE DATA
        //================================================//

        [Serializable]
        public class QuestDialogueData
        {
            public DialogueData dialogue;

            public DialogueReward reward;
        }


        //================================================//
        // INFO
        //================================================//

        [Header("Info")]

        [SerializeField]
        private string questID;

        [SerializeField]
        private string questName;

        [TextArea(4, 6)]
        [SerializeField]
        private string description;


        //================================================//
        // OBJECTIVES
        //================================================//

        [Header("Objectives")]

        [SerializeField]
        private List<QuestObjectiveData> objectives = new();


        //================================================//
        // REWARDS
        //================================================//

        [Header("Mission Rewards")]

        [SerializeField]
        private List<RewardData> rewards = new();


        //================================================//
        // DIALOGUE
        //================================================//

        [Header("Dialogue")]

        [SerializeField]
        private QuestDialogueData startDialogue;

        [SerializeField]
        private QuestDialogueData inProgressDialogue;

        [SerializeField]
        private QuestDialogueData completionDialogue;


        //================================================//
        // SETTINGS
        //================================================//

        [Header("Settings")]

        [SerializeField]
        private bool repeatable;

        [SerializeField]
        private bool autoComplete;


        //================================================//
        // GETTERS
        //================================================//

        public string QuestID => questID;

        public string QuestName => questName;

        public string Description => description;

        public QuestType Type => questType;

        public IReadOnlyList<QuestObjectiveData> Objectives =>
            objectives;

        public IReadOnlyList<RewardData> Rewards =>
            rewards;


        //================================================//
        // DIALOGUE GETTERS
        //================================================//

        public QuestDialogueData StartDialogueData =>
            startDialogue;

        public QuestDialogueData InProgressDialogueData =>
            inProgressDialogue;

        public QuestDialogueData CompletionDialogueData =>
            completionDialogue;


        //================================================//
        // DIRECT DIALOGUE GETTERS
        //================================================//

        public DialogueData StartDialogue =>
            startDialogue?.dialogue;

        public DialogueData InProgressDialogue =>
            inProgressDialogue?.dialogue;

        public DialogueData CompletionDialogue =>
            completionDialogue?.dialogue;


        //================================================//
        // SETTINGS GETTERS
        //================================================//

        public bool Repeatable => repeatable;

        public bool AutoComplete => autoComplete;
    }
}

