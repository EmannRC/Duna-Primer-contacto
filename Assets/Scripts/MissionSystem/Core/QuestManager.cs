using System.Collections.Generic;
using UnityEngine;
using System;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Administra todas las misiones de un jugador.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField]
        private QuestDatabase questDatabase;

        [SerializeField] private string startingQuestID;

        [Header("Rewards")]
        [SerializeField]
        private QuestRewardManager rewardManager;


        private readonly List<QuestInstance> activeQuests = new();

        private readonly List<QuestInstance> completedQuests = new();


        public IReadOnlyList<QuestInstance>
            ActiveQuests =>
            activeQuests;


        public IReadOnlyList<QuestInstance>
            CompletedQuests =>
            completedQuests;

        
        private void Start()
        {
            AcceptQuest(startingQuestID);
        }
        

        //================================================//
        // EVENTS
        //================================================//

        public event Action<QuestInstance>
            OnQuestAccepted;


        public event Action<QuestInstance>
            OnQuestCompleted;


        public event Action<QuestInstance>
            OnQuestTurnedIn;


        public event Action<QuestInstance>
            OnQuestAbandoned;


        public event Action<QuestInstance>
            OnObjectiveUpdated;


        private void OnEnable()
        {
            QuestEvents.OnTalkToNPC += HandleTalkToNPC;
            QuestEvents.OnCollectItem += HandleCollectItem;
            QuestEvents.OnDeliverItem += HandleDeliverItem;
            QuestEvents.OnKillEnemy += HandleKillEnemy;
            QuestEvents.OnReachArea += HandleReachArea;
        }

        private void OnDisable()
        {
            QuestEvents.OnTalkToNPC -= HandleTalkToNPC;
            QuestEvents.OnCollectItem -= HandleCollectItem;
            QuestEvents.OnDeliverItem -= HandleDeliverItem;
            QuestEvents.OnKillEnemy -= HandleKillEnemy;
            QuestEvents.OnReachArea -= HandleReachArea;
        }

        private void HandleCollectItem(string itemID, int amount)
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.TryProgress(
                    ObjectiveType.Collect,
                    itemID,
                    amount))
                {
                    NotifyObjectiveUpdated(quest);
                }
            }
        }

        private void HandleTalkToNPC(string npcID)
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.TryProgress(
                    ObjectiveType.Talk,
                    npcID))
                {
                    NotifyObjectiveUpdated(quest);
                }
            }
        }

        private void HandleDeliverItem(string itemID, int amount)
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.TryProgress(
                    ObjectiveType.Deliver,
                    itemID,
                    amount))
                {
                    NotifyObjectiveUpdated(quest);
                }
            }
        }

        private void HandleKillEnemy(string enemyID, int amount)
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.TryProgress(
                    ObjectiveType.Kill,
                    enemyID,
                    amount))
                {
                    NotifyObjectiveUpdated(quest);
                }
            }
        }

        private void HandleReachArea(string areaID)
        {
            foreach (QuestInstance quest in activeQuests)
            {
                if (quest.TryProgress(
                    ObjectiveType.ReachArea,
                    areaID))
                {
                    NotifyObjectiveUpdated(quest);
                }
            }
        }

        //================================================//
        // ACCEPT QUEST
        //================================================//

        public bool AcceptQuest(
            string questID)
        {
            if (questDatabase == null)
            {
                Debug.LogError("QuestManager no tiene una QuestDatabase asignada.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(questID))
            {
                Debug.LogWarning("No se puede aceptar una misión sin ID.");
                return false;
            }

            QuestData questData =
                questDatabase.GetQuest(
                    questID
                );


            if (questData == null)
            {
                Debug.LogWarning(
                    $"Quest '{questID}' no existe."
                );

                return false;
            }


            if (HasQuest(questID))
            {
                Debug.LogWarning(
                    $"La misión '{questID}' ya está activa."
                );

                return false;
            }

            if (!questData.Repeatable && IsQuestCompleted(questID))
            {
                Debug.LogWarning(
                    $"La misión '{questID}' ya fue completada y no es repetible."
                );

                return false;
            }


            QuestInstance quest =
                new QuestInstance(
                    questData
                );


            activeQuests.Add(
                quest
            );


            OnQuestAccepted?.Invoke(
                quest
            );


            // Una misión sin objetivos ya está completa desde su creación.
            // Así las misiones automáticas no quedan activas para siempre.
            if (quest.IsCompleted)
            {
                OnQuestCompleted?.Invoke(quest);

                if (quest.Data.AutoComplete)
                {
                    TurnInQuest(quest.Data.QuestID);
                }
            }


            return true;
        }


        //================================================//
        // ABANDON QUEST
        //================================================//

        public bool AbandonQuest(
            string questID)
        {
            QuestInstance quest =
                GetActiveQuest(
                    questID
                );


            if (quest == null)
                return false;


            activeQuests.Remove(
                quest
            );


            OnQuestAbandoned?.Invoke(
                quest
            );


            return true;
        }


        //================================================//
        // TURN IN QUEST
        //================================================//

        public bool TurnInQuest(
            string questID)
        {
            QuestInstance quest =
                GetActiveQuest(
                    questID
                );


            if (quest == null)
                return false;


            if (!quest.IsCompleted)
                return false;


            if (rewardManager != null)
            {
                rewardManager.GiveRewards(
                    quest.Data
                );
            }


            quest.TurnIn();


            activeQuests.Remove(
                quest
            );


            completedQuests.Add(
                quest
            );


            OnQuestTurnedIn?.Invoke(
                quest
            );


            return true;
        }


        //================================================//
        // CHECK COMPLETED QUESTS
        //================================================//

        public void CheckCompletedQuests()
        {
            foreach (
                QuestInstance quest
                in activeQuests
            )
            {
                if (
                    quest.State
                    != QuestState.Completed
                )
                {
                    continue;
                }


                OnQuestCompleted?.Invoke(
                    quest
                );
            }
        }


        //================================================//
        // QUERIES
        //================================================//

        public bool HasQuest(
            string questID)
        {
            return GetActiveQuest(
                questID
            ) != null;
        }


        public QuestInstance GetActiveQuest(
            string questID)
        {
            foreach (
                QuestInstance quest
                in activeQuests
            )
            {
                if (
                    quest.Data.QuestID
                    == questID
                )
                {
                    return quest;
                }
            }


            return null;
        }


        public QuestInstance GetCompletedQuest(
            string questID)
        {
            foreach (
                QuestInstance quest
                in completedQuests
            )
            {
                if (
                    quest.Data.QuestID
                    == questID
                )
                {
                    return quest;
                }
            }


            return null;
        }


        public bool IsQuestCompleted(
            string questID)
        {
            return GetCompletedQuest(
                questID
            ) != null;
        }


        //================================================//
        // OBJECTIVE UPDATED
        //================================================//

        public void NotifyObjectiveUpdated(
            QuestInstance quest)
        {
            if (quest == null)
                return;

            OnObjectiveUpdated?.Invoke(
                quest
            );


            if (
                quest.State
                == QuestState.Completed
            )
            {
                OnQuestCompleted?.Invoke(
                    quest
                );

                if (quest.Data.AutoComplete)
                {
                    TurnInQuest(quest.Data.QuestID);
                }
            }
        }
    }
}
