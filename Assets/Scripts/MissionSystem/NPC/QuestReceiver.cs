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
        public bool CanTurnIn()
        {
            if (quest == null)
                return false;


            QuestManager manager = FindFirstObjectByType<QuestManager>();

            if (manager == null)
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
        public void TurnInQuest()
        {
            if (!CanTurnIn())
            {
                Debug.Log(
                    "La misión todavía no está completada."
                );

                return;
            }


            QuestManager manager =
                FindFirstObjectByType<QuestManager>();


            bool success =
                manager.TurnInQuest(quest.QuestID);


            if (success)
            {
                Debug.Log(
                    $"Misión entregada: {quest.QuestName}"
                );
            }
        }


        /// <summary>
        /// Método pensado para el sistema de diálogo.
        /// </summary>
        public void Interact()
        {
            if (!HasQuest())
                return;


            if (CanTurnIn())
            {
                TurnInQuest();
            }
            else
            {
                Debug.Log(
                    "Este NPC no tiene nada para recibir."
                );
            }
        }
    }
}
