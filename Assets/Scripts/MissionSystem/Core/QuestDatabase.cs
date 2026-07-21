using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "Duna/Quest System/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        [SerializeField] private List<QuestData> quests = new();

        public IReadOnlyList<QuestData> Quests => quests;

        public QuestData GetQuest(string questID)
        {
            foreach (QuestData quest in quests)
            {
                if (quest.QuestID == questID)
                    return quest;
            }

            return null;
        }
    }
}