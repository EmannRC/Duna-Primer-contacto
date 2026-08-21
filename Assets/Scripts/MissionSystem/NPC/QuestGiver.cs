using UnityEngine;

namespace Duna.QuestSystem
{
    public class QuestGiver : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField]
        private QuestData quest;


        public QuestData Quest => quest;


        public bool HasQuest()
        {
            return quest != null;
        }


        public bool GiveQuest(
            QuestManager questManager)
        {
            if (quest == null)
            {
                Debug.LogWarning(
                    $"El NPC {name} no tiene una misión."
                );

                return false;
            }


            if (questManager == null)
            {
                Debug.LogError(
                    "QuestManager inválido."
                );

                return false;
            }


            return questManager.AcceptQuest(
                quest.QuestID
            );
        }


        public void Interact(
            QuestManager questManager)
        {
            if (!HasQuest())
                return;


            GiveQuest(
                questManager
            );
        }
    }
}