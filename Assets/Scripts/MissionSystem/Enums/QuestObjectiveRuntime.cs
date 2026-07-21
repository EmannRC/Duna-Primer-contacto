using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Representa el progreso en tiempo de ejecución de un objetivo de misión.
    /// </summary>
    [System.Serializable]
    public class QuestObjectiveRuntime
    {
        [SerializeField] private QuestObjectiveData data;
        [SerializeField] private int currentAmount;
        [SerializeField] private QuestObjectiveState state;

        public QuestObjectiveData Data => data;
        public int CurrentAmount => currentAmount;
        public QuestObjectiveState State => state;

        public bool IsCompleted => state == QuestObjectiveState.Completed;

        /// <summary>
        /// Devuelve el progreso normalizado entre 0 y 1.
        /// </summary>
        public float ProgressNormalized =>
            data.RequiredAmount <= 0
                ? 1f
                : (float)currentAmount / data.RequiredAmount;

        public QuestObjectiveRuntime(QuestObjectiveData data)
        {
            this.data = data;

            currentAmount = 0;
            state = QuestObjectiveState.Inactive;
        }

        /// <summary>
        /// Activa el objetivo para que pueda recibir progreso.
        /// </summary>
        public void Activate()
        {
            if (state != QuestObjectiveState.Inactive)
                return;

            state = QuestObjectiveState.Active;
        }

        /// <summary>
        /// Intenta aplicar progreso al objetivo.
        /// Devuelve true si el evento corresponde a este objetivo.
        /// </summary>
        public bool TryProgress(ObjectiveType type, string targetID, int amount = 1)
        {
            if (state != QuestObjectiveState.Active)
                return false;

            if (data.ObjectiveType != type)
                return false;

            if (data.TargetID != targetID)
                return false;

            currentAmount += amount;

            if (currentAmount >= data.RequiredAmount)
            {
                currentAmount = data.RequiredAmount;
                state = QuestObjectiveState.Completed;
            }

            return true;
        }

        /// <summary>
        /// Completa el objetivo inmediatamente.
        /// </summary>
        public void ForceComplete()
        {
            currentAmount = data.RequiredAmount;
            state = QuestObjectiveState.Completed;
        }

        /// <summary>
        /// Reinicia el progreso del objetivo.
        /// </summary>
        public void Reset()
        {
            currentAmount = 0;
            state = QuestObjectiveState.Inactive;
        }
    }
}