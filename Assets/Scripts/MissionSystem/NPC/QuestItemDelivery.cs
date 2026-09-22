using Duna.InteractionSystem;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Permite a un NPC recibir todos los objetos
    /// requeridos por un objetivo de misión de tipo Deliver.
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

            if (objective.Data.RequiredItems == null ||
                objective.Data.RequiredItems.Count == 0)
            {
                return false;
            }

            // Comprobar que el jugador tenga TODOS
            // los objetos necesarios.
            foreach (var requiredItem
                     in objective.Data.RequiredItems)
            {
                if (requiredItem == null)
                    return false;

                if (!inventory.HasItem(
                        requiredItem.ItemID,
                        requiredItem.Amount))
                {
                    return false;
                }
            }

            return true;
        }


        //================================================//
        // DELIVER
        //================================================//

        public bool Deliver(
            QuestManager questManager,
            IQuestItemProvider inventory)
        {
            if (!CanDeliver(
                    questManager,
                    inventory))
            {
                Debug.Log(
                    "No se pueden entregar todos los objetos requeridos.");

                return false;
            }

            QuestInstance questInstance =
                questManager.GetActiveQuest(
                    quest.QuestID);

            QuestObjectiveRuntime objective =
                questInstance.CurrentObjective;

            // Retirar TODOS los objetos de una sola operación.
            if (!inventory.TryRemoveItems(
                    objective.Data.RequiredItems))
            {
                Debug.LogError(
                    "No se pudieron retirar todos los objetos " +
                    "de la misión.");

                return false;
            }

            // Avisar al sistema de quests que se realizó
            // la entrega de cada requisito.
            foreach (var requiredItem
                     in objective.Data.RequiredItems)
            {
                QuestEvents.RaiseDeliverItem(
                    requiredItem.ItemID,
                    requiredItem.Amount);
            }

            return true;
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

