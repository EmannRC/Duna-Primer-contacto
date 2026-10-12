using Duna.InteractionSystem;
using System;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Permite a un NPC recibir el objeto único requerido
    /// por un objetivo de misión de tipo Deliver.
    /// </summary>
    public class QuestItemDelivery : MonoBehaviour, IInteractable
    {
        [Header("Quest")]
        [SerializeField]
        private QuestData quest;


        //================================================//
        // GETTERS
        //================================================//

        public QuestData Quest => quest;


        //================================================//
        // CAN DELIVER
        //================================================//

        public bool CanDeliver(
            QuestManager questManager,
            IQuestItemProvider inventory)
        {
            if (quest == null ||
                questManager == null ||
                inventory == null)
            {
                return false;
            }

            QuestInstance questInstance =
                questManager.GetActiveQuest(quest.QuestID);

            if (questInstance == null)
                return false;

            QuestObjectiveRuntime objective =
                questInstance.CurrentObjective;

            if (objective == null)
                return false;

            if (objective.Data.ObjectiveType != ObjectiveType.Deliver)
                return false;

            string itemID = objective.Data.TargetID;
            int amount = objective.Data.RequiredAmount;

            if (string.IsNullOrWhiteSpace(itemID) || amount <= 0)
            {
                return false;
            }

            return inventory.HasItem(itemID, amount);
        }


        //================================================//
        // DELIVER
        //================================================//

        public void Deliver(
            QuestManager questManager,
            Inventory inventory,
            Action<bool> onCompleted = null)
        {
            if (!CanDeliver(
                    questManager,
                    inventory))
            {
                Debug.Log(
                    "No se puede entregar el objeto requerido.");

                onCompleted?.Invoke(false);
                return;
            }

            QuestInstance questInstance =
                questManager.GetActiveQuest(
                    quest.QuestID);

            QuestObjectiveRuntime objective =
                questInstance.CurrentObjective;

            string itemID = objective.Data.TargetID;
            int amount = objective.Data.RequiredAmount;

            inventory.RequestRemoveItem(itemID, amount, removed =>
            {
                if (!removed)
                {
                    Debug.LogError(
                        "No se pudo retirar el objeto requerido " +
                        "de la misión.");
                    onCompleted?.Invoke(false);
                    return;
                }

                QuestEvents.RaiseDeliverItem(itemID, amount);
                onCompleted?.Invoke(true);
            });

        }


        //================================================//
        // INTERACTION
        //================================================//

        public void Interact(GameObject player)
        {
            PlayerContext context =
                player != null
                    ? player.GetComponent<PlayerContext>()
                    : null;

            if (context == null)
            {
                Debug.LogError(
                    "La entrega de misión requiere " +
                    "un PlayerContext válido.");

                return;
            }

            Deliver(
                context.questManager,
                context.inventory);
        }
    }
}

