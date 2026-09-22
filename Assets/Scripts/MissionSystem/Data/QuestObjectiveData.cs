using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    [System.Serializable]
    public class QuestObjectiveData
    {
        //================================================//
        // REQUIRED ITEM
        //================================================//

        [System.Serializable]
        public class RequiredItem
        {
            [SerializeField]
            private string itemID;

            [SerializeField]
            private int amount = 1;

            public string ItemID => itemID;
            public int Amount => amount;
        }


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


        //================================================//
        // DELIVER ITEMS
        //================================================//

        [Header("Deliver Items")]

        [SerializeField]
        private List<RequiredItem> requiredItems = new();


        //================================================//
        // GETTERS
        //================================================//

        public ObjectiveType ObjectiveType => objectiveType;

        public string TargetID => targetID;

        public int RequiredAmount => requiredAmount;

        public string Description => description;

        public IReadOnlyList<RequiredItem> RequiredItems =>
            requiredItems;
    }
}

