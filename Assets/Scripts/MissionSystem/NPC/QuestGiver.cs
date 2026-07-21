using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Componente que permite a un NPC entregar misiones.
    /// </summary>
    public class QuestGiver : MonoBehaviour
    {
        [Header("Quest Data")]
        [SerializeField] private QuestData quest;

        [Header("Settings")]
        [SerializeField] private bool autoAccept = false;


        public QuestData Quest => quest;


        /// <summary>
        /// Comprueba si el NPC tiene una misión disponible.
        /// </summary>
        public bool HasQuest()
        {
            return quest != null;
        }


        /// <summary>
        /// Intenta entregar la misión al jugador.
        /// </summary>
        public void GiveQuest()
        {
            if (quest == null)
            {
                Debug.LogWarning(
                    $"El NPC {name} no tiene una misión asignada."
                );

                return;
            }


            QuestManager manager = FindFirstObjectByType<QuestManager>();

            if (manager == null)
            {
                Debug.LogError(
                    "No existe un QuestManager en la escena."
                );

                return;
            }


            bool accepted = manager.AcceptQuest(quest.QuestID);


            if (accepted)
            {
                Debug.Log(
                    $"Misión aceptada: {quest.QuestName}"
                );
            }
        }


        /// <summary>
        /// Método pensado para conectarlo al sistema de diálogo.
        /// </summary>
        public void Interact()
        {
            if (!HasQuest())
                return;


            if (autoAccept)
            {
                GiveQuest();
            }
            else
            {
                // Aquí después abriremos la UI de aceptar misión.
                Debug.Log(
                    $"NPC ofrece misión: {quest.QuestName}"
                );
            }
        }
    }
}
