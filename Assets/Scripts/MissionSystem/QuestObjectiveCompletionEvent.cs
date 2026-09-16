using UnityEngine;
using UnityEngine.Events;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Ejecuta acciones de escena al completar un objetivo concreto de una misión.
    /// Se configura en el Inspector y permite asignar cualquier método público,
    /// como EnemySpawner.SpawnEnemies o GameObject.SetActive.
    /// </summary>
    public class QuestObjectiveCompletionEvent : MonoBehaviour
    {
        [Header("Objective")]
        [Tooltip("Asset de la misión que contiene el objetivo a escuchar.")]
        [SerializeField] private QuestData quest;

        [Tooltip("Posición del objetivo dentro de la lista Objectives de la misión. El primero es 0.")]
        [Min(0)]
        [SerializeField] private int objectiveIndex;

        [Header("Event")]
        [SerializeField] private UnityEvent onObjectiveCompleted;

        private QuestManager questManager;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Lo llama LocalPlayerBootstrap para enlazar el gestor del jugador local.
        /// </summary>
        public void Initialize(QuestManager manager)
        {
            if (questManager == manager)
                return;

            Unsubscribe();
            questManager = manager;
            Subscribe();
        }

        private void Subscribe()
        {
            if (questManager == null || !isActiveAndEnabled)
                return;

            questManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
            questManager.OnObjectiveCompleted += HandleObjectiveCompleted;
        }

        private void Unsubscribe()
        {
            if (questManager != null)
            {
                questManager.OnObjectiveCompleted -= HandleObjectiveCompleted;
            }
        }

        private void HandleObjectiveCompleted(
            QuestInstance completedQuest,
            int completedObjectiveIndex,
            QuestObjectiveRuntime completedObjective)
        {
            if (
                completedQuest == null ||
                completedQuest.Data != quest ||
                completedObjectiveIndex != objectiveIndex
            )
            {
                return;
            }

            onObjectiveCompleted?.Invoke();
        }
    }
}
