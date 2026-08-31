using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Representa una instancia de una misión para un jugador.
    /// Contiene el progreso en tiempo de ejecución.
    /// </summary>
    [System.Serializable]
    public class QuestInstance
    {
        private readonly QuestData data;
        private readonly List<QuestObjectiveRuntime> objectives = new();

        private int currentObjectiveIndex;
        private QuestState state;

        public QuestData Data => data;
        public QuestState State => state;
        public int CurrentObjectiveIndex => currentObjectiveIndex;

        public IReadOnlyList<QuestObjectiveRuntime> Objectives => objectives;

        public QuestObjectiveRuntime CurrentObjective =>
            objectives.Count > 0 && currentObjectiveIndex < objectives.Count
                ? objectives[currentObjectiveIndex]
                : null;

        public bool IsCompleted =>
            state == QuestState.Completed ||
            state == QuestState.TurnedIn;

        public QuestInstance(QuestData data)
        {
            this.data = data;

            state = QuestState.Active;
            currentObjectiveIndex = 0;

            foreach (QuestObjectiveData objectiveData in data.Objectives)
            {
                objectives.Add(new QuestObjectiveRuntime(objectiveData));
            }

            if (objectives.Count > 0)
            {
                objectives[0].Activate();
            }
            else
            {
                state = QuestState.Completed;
            }
        }

        public bool IsCurrentObjectiveTalkTo(string npcID)
        {
            if (CurrentObjective == null)
                return false;

            return CurrentObjective.Data.ObjectiveType == ObjectiveType.Talk &&
                   CurrentObjective.Data.TargetID == npcID &&
                   !CurrentObjective.IsCompleted;
        }

        /// <summary>
        /// Intenta avanzar el objetivo activo.
        /// </summary>
        public bool TryProgress(ObjectiveType type, string targetID, int amount = 1)
        {
            if (CurrentObjective == null)
                return false;

            bool progressed = CurrentObjective.TryProgress(type, targetID, amount);

            if (!progressed)
                return false;

            if (CurrentObjective.IsCompleted)
            {
                AdvanceObjective();
            }

            return true;
        }

        /// <summary>
        /// Activa el siguiente objetivo.
        /// </summary>
        private void AdvanceObjective()
        {
            currentObjectiveIndex++;

            if (currentObjectiveIndex >= objectives.Count)
            {
                state = QuestState.Completed;
                return;
            }

            objectives[currentObjectiveIndex].Activate();
        }

        public void TurnIn()
        {
            if (state == QuestState.Completed)
            {
                state = QuestState.TurnedIn;
            }
        }
    }
}
