using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Permite a un NPC recibir un objeto relacionado con una misión.
    /// </summary>
    public class QuestItemDelivery : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private QuestData quest;

        [Header("Item")]
        [SerializeField] private string itemID;

        [SerializeField] private int requiredAmount = 1;

        public QuestData Quest => quest;

        public string ItemID => itemID;

        public int RequiredAmount => requiredAmount;

        public bool CanDeliver()
        {
            if (quest == null)
                return false;

            QuestManager questManager =
                FindFirstObjectByType<QuestManager>();

            if (questManager == null)
                return false;

            QuestInstance questInstance =
                questManager.GetActiveQuest(quest.QuestID);

            if (questInstance == null)
                return false;

            if (questInstance.CurrentObjective == null)
                return false;

            QuestObjectiveRuntime objective =
                questInstance.CurrentObjective;

            if (objective.Data.ObjectiveType != ObjectiveType.Deliver)
                return false;

            if (objective.Data.TargetID != itemID)
                return false;

            return true;
        }

        public void Deliver()
        {
            if (!CanDeliver())
            {
                Debug.Log("No se puede entregar este objeto.");
                return;
            }

            QuestEvents.RaiseDeliverItem(
                itemID,
                requiredAmount
            );
        }
    }
}
