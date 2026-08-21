using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Componente que permite a un NPC recibir misiones completadas.
    /// </summary>
    public class QuestReceiver : MonoBehaviour
    {
        [Header("Quest Data")]
        [SerializeField] private QuestData quest;


        public QuestData Quest => quest;


        /// <summary>
        /// Comprueba si este NPC puede recibir una misión.
        /// </summary>
        public bool HasQuest()
        {
            return quest != null;
        }


        /// <summary>
        /// Comprueba si la misión está lista para entregar.
        /// </summary>
        public bool CanTurnIn(QuestManager manager)
        {
            if (quest == null || manager == null)
                return false;


            QuestInstance instance =
                manager.GetActiveQuest(quest.QuestID);


            if (instance == null)
                return false;


            return instance.State == QuestState.Completed;
        }


        /// <summary>
        /// Entrega la misión y finaliza la recompensa.
        /// </summary>
        public bool TurnInQuest(QuestManager manager)
        {
            if (!CanTurnIn(manager))
            {
                Debug.Log(
                    "La misión todavía no está completada."
                );

                return false;
            }

            bool success =
                manager.TurnInQuest(quest.QuestID);


            if (success)
            {
                Debug.Log(
                    $"Misión entregada: {quest.QuestName}"
                );
            }

            return success;
        }


        /// <summary>
        /// Método pensado para el sistema de diálogo.
        /// </summary>
        public bool Interact(QuestManager manager)
        {
            if (!HasQuest())
                return false;


            if (CanTurnIn(manager))
            {
                return TurnInQuest(manager);
            }

            Debug.Log(
                "Este NPC no tiene nada para recibir."
            );

            return false;
        }
    }
}
