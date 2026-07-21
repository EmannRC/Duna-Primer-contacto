using System.Collections.Generic;
using UnityEngine;
using System;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Administra todas las misiones del jugador.
    /// No escucha eventos del juego; esa responsabilidad pertenece al QuestTracker.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private QuestDatabase questDatabase;

        private readonly List<QuestInstance> activeQuests = new();
        private readonly List<QuestInstance> completedQuests = new();

        /// <summary>
        /// Misiones activas.
        /// </summary>
        public IReadOnlyList<QuestInstance> ActiveQuests => activeQuests;

        /// <summary>
        /// Misiones completadas.
        /// </summary>
        public IReadOnlyList<QuestInstance> CompletedQuests => completedQuests;

        //------------------------------------------------//
        // Events
        //------------------------------------------------//

        public event Action<QuestInstance> OnQuestAccepted;
        public event Action<QuestInstance> OnQuestCompleted;
        public event Action<QuestInstance> OnQuestTurnedIn;
        public event Action<QuestInstance> OnQuestAbandoned;

        public event Action<QuestInstance> OnObjectiveUpdated;

        //------------------------------------------------//
        // Quest Management
        //------------------------------------------------//

        /// <summary>
        /// Acepta una misión.
        /// </summary>
        public bool AcceptQuest(string questID)
        {
            QuestData questData = questDatabase.GetQuest(questID);

            if (questData == null)
            {
                Debug.LogWarning($"Quest '{questID}' no existe.");
                return false;
            }

            if (HasQuest(questID))
            {
                Debug.LogWarning($"La misión '{questID}' ya está activa.");
                return false;
            }

            QuestInstance quest = new QuestInstance(questData);

            activeQuests.Add(quest);

            OnQuestAccepted?.Invoke(quest);

            return true;
        }

        /// <summary>
        /// Abandona una misión activa.
        /// </summary>
        public bool AbandonQuest(string questID)
        {
            QuestInstance quest = GetActiveQuest(questID);

            if (quest == null)
                return false;

            activeQuests.Remove(quest);

            OnQuestAbandoned?.Invoke(quest);

            return true;
        }

        /// <summary>
        /// Marca una misión como entregada.
        /// </summary>
        public bool TurnInQuest(string questID)
        {
            QuestInstance quest = GetActiveQuest(questID);

            if (quest == null)
                return false;


            if (!quest.IsCompleted)
                return false;


            QuestRewardManager rewardManager =
                FindFirstObjectByType<QuestRewardManager>();


            if (rewardManager != null)
            {
                rewardManager.GiveRewards(
                    quest.Data
                );
            }


            quest.TurnIn();


            activeQuests.Remove(quest);

            completedQuests.Add(quest);


            OnQuestTurnedIn?.Invoke(quest);


            return true;
        }

        //------------------------------------------------//
        // Update
        //------------------------------------------------//

        /// <summary>
        /// Comprueba qué misiones han sido completadas.
        /// Lo llamará QuestTracker después de actualizar el progreso.
        /// </summary>
        public void CheckCompletedQuests()
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.State != QuestState.Completed)
                    continue;

                OnQuestCompleted?.Invoke(quest);
            }
        }

        //------------------------------------------------//
        // Queries
        //------------------------------------------------//

        public bool HasQuest(string questID)
        {
            return GetActiveQuest(questID) != null;
        }

        public QuestInstance GetActiveQuest(string questID)
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.Data.QuestID == questID)
                    return quest;
            }

            return null;
        }

        public QuestInstance GetCompletedQuest(string questID)
        {
            foreach (QuestInstance quest in completedQuests)
            {
                if (quest.Data.QuestID == questID)
                    return quest;
            }

            return null;
        }

        public bool IsQuestCompleted(string questID)
        {
            return GetCompletedQuest(questID) != null;
        }

        public void NotifyObjectiveUpdated(QuestInstance quest)
        {
            OnObjectiveUpdated?.Invoke(quest);
        }
    }
}
