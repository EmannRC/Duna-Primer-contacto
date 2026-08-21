using Duna.InteractionSystem;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Permite a un NPC recibir un objeto relacionado con una misión.
    /// </summary>
    public class QuestItemDelivery : MonoBehaviour, IInteractable
    {
        [Header("Quest")]
        [SerializeField] private QuestData quest;

        [Header("Item")]
        [SerializeField] private string itemID;

        [SerializeField] private int requiredAmount = 1;

        public QuestData Quest => quest;

        public string ItemID => itemID;

        public int RequiredAmount => requiredAmount;

        public bool CanDeliver(QuestManager questManager, IQuestItemProvider inventory)
        {
            if (quest == null || questManager == null || inventory == null || requiredAmount <= 0)
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

            int amountToDeliver = Mathf.Min(
                requiredAmount,
                objective.Data.RequiredAmount - objective.CurrentAmount
            );

            return amountToDeliver > 0 && inventory.HasItem(itemID, amountToDeliver);
        }

        public bool Deliver(QuestManager questManager, IQuestItemProvider inventory)
        {
            if (!CanDeliver(questManager, inventory))
            {
                Debug.Log("No se puede entregar este objeto.");
                return false;
            }

            QuestObjectiveRuntime objective = questManager.GetActiveQuest(quest.QuestID).CurrentObjective;
            int amountToDeliver = Mathf.Min(
                requiredAmount,
                objective.Data.RequiredAmount - objective.CurrentAmount
            );

            if (!inventory.TryRemoveItem(itemID, amountToDeliver))
                return false;

            QuestEvents.RaiseDeliverItem(
                itemID,
                amountToDeliver
            );

            return true;
        }

        public void Interact(GameObject player)
        {
            PlayerContext context = player != null ? player.GetComponent<PlayerContext>() : null;

            if (context == null)
            {
                Debug.LogError("La entrega de misión requiere un PlayerContext válido.");
                return;
            }

            Deliver(context.questManager, context.inventory);
        }
    }
}
