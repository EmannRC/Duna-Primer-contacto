using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Componente que permite a un NPC recibir misiones.
    /// </summary>
    public class QuestReceiver : MonoBehaviour
    {
        [Header("Quest Data")]
        [SerializeField]
        private QuestData quest;


        public QuestData Quest => quest;


        //================================================//
        // HAS QUEST
        //================================================//

        public bool HasQuest()
        {
            return quest != null;
        }


        //================================================//
        // CAN DELIVER
        //================================================//

        /// <summary>
        /// Comprueba si el objetivo actual de la misión
        /// consiste en entregar un objeto.
        /// </summary>
        public bool CanDeliver(
            QuestManager manager
        )
        {
            if (
                quest == null ||
                manager == null
            )
            {
                return false;
            }


            QuestInstance instance =
                manager.GetActiveQuest(
                    quest.QuestID
                );


            if (instance == null)
                return false;


            QuestObjectiveRuntime objective =
                instance.CurrentObjective;


            if (objective == null)
                return false;


            return
                objective.Data.ObjectiveType
                == ObjectiveType.Deliver;
        }


        //================================================//
        // GET DELIVERY ITEM ID
        //================================================//

        public string GetDeliveryItemID(
            QuestManager manager
        )
        {
            if (!CanDeliver(manager))
                return null;


            QuestInstance instance =
                manager.GetActiveQuest(
                    quest.QuestID
                );


            return instance
                .CurrentObjective
                .Data
                .TargetID;
        }


        //================================================//
        // DELIVER QUEST ITEM
        //================================================//

        /// <summary>
        /// Registra la entrega del objeto para completar
        /// el objetivo Deliver.
        /// </summary>
        public bool DeliverItem(
            QuestManager manager
        )
        {
            if (!CanDeliver(manager))
                return false;


            QuestInstance instance =
                manager.GetActiveQuest(
                    quest.QuestID
                );


            QuestObjectiveRuntime objective =
                instance.CurrentObjective;


            string itemID =
                objective.Data.TargetID;


            int amount =
                objective.Data.RequiredAmount;


            QuestEvents.RaiseDeliverItem(
                itemID,
                amount
            );


            return true;
        }


        //================================================//
        // CAN TURN IN
        //================================================//

        public bool CanTurnIn(
            QuestManager manager
        )
        {
            if (
                quest == null ||
                manager == null
            )
            {
                return false;
            }


            QuestInstance instance =
                manager.GetActiveQuest(
                    quest.QuestID
                );


            if (instance == null)
                return false;


            return instance.State ==
                   QuestState.Completed;
        }


        //================================================//
        // TURN IN QUEST
        //================================================//

        public bool TurnInQuest(
            QuestManager manager
        )
        {
            if (!CanTurnIn(manager))
            {
                Debug.Log(
                    "La misión todavía no está completada."
                );

                return false;
            }


            bool success =
                manager.TurnInQuest(
                    quest.QuestID
                );


            if (success)
            {
                Debug.Log(
                    $"Misión entregada: {quest.QuestName}"
                );
            }


            return success;
        }
    }
}