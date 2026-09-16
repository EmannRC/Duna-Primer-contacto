using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace Duna.QuestSystem.UI
{
    public class QuestTrackerUI : MonoBehaviour
    {
        [Header("Quest Manager")]
        [SerializeField]
        private QuestManager questManager;


        [Header("Main Quest")]
        [SerializeField]
        private QuestEntryUI mainQuestEntry;


        [Header("Side Quests")]
        [SerializeField]
        private QuestEntryUI sideQuestEntry1;

        [SerializeField]
        private QuestEntryUI sideQuestEntry2;

        [SerializeField]
        private QuestEntryUI sideQuestEntry3;


        private bool isBound;


        //==========================================================//
        // BIND
        //==========================================================//

        public void Bind(QuestManager manager)
        {
            if (manager == null)
            {
                Debug.LogError(
                    "QuestTrackerUI recibió un QuestManager NULL."
                );

                return;
            }

            if (isBound)
            {
                Unbind();
            }

            questManager = manager;

            SubscribeToEvents();

            isBound = true;

            RefreshUI(null);
        }


        //==========================================================//
        // UNBIND
        //==========================================================//

        private void Unbind()
        {
            if (questManager == null)
                return;

            questManager.OnQuestAccepted
                -= RefreshUI;

            questManager.OnQuestCompleted
                -= RefreshUI;

            questManager.OnQuestTurnedIn
                -= RefreshUI;

            questManager.OnObjectiveUpdated
                -= RefreshUI;

            isBound = false;
        }


        //==========================================================//
        // DESTROY
        //==========================================================//

        private void OnDestroy()
        {
            Unbind();
        }


        //==========================================================//
        // EVENTS
        //==========================================================//

        private void SubscribeToEvents()
        {
            questManager.OnQuestAccepted
                += RefreshUI;

            questManager.OnQuestCompleted
                += RefreshUI;

            questManager.OnQuestTurnedIn
                += RefreshUI;

            questManager.OnObjectiveUpdated
                += RefreshUI;
        }


        //==========================================================//
        // REFRESH UI
        //==========================================================//

        private void RefreshUI(QuestInstance quest)
        {
            if (questManager == null)
                return;

            ClearEntries();

            foreach (QuestInstance activeQuest
                in questManager.ActiveQuests)
            {
                if (activeQuest == null)
                    continue;

                if (activeQuest.Data.Type ==
                    QuestData.QuestType.Main)
                {
                    if (mainQuestEntry != null)
                    {
                        mainQuestEntry.SetQuest(activeQuest);
                    }

                    continue;
                }

                AddSideQuest(activeQuest);
            }
        }


        //==========================================================//
        // ADD SIDE QUEST
        //==========================================================//

        private void AddSideQuest(QuestInstance quest)
        {
            if (sideQuestEntry1 != null &&
                sideQuestEntry1.IsEmpty)
            {
                sideQuestEntry1.SetQuest(quest);
                return;
            }

            if (sideQuestEntry2 != null &&
                sideQuestEntry2.IsEmpty)
            {
                sideQuestEntry2.SetQuest(quest);
                return;
            }

            if (sideQuestEntry3 != null &&
                sideQuestEntry3.IsEmpty)
            {
                sideQuestEntry3.SetQuest(quest);
                return;
            }
        }


        //==========================================================//
        // CLEAR
        //==========================================================//

        private void ClearEntries()
        {
            if (mainQuestEntry != null)
                mainQuestEntry.Clear();

            if (sideQuestEntry1 != null)
                sideQuestEntry1.Clear();

            if (sideQuestEntry2 != null)
                sideQuestEntry2.Clear();

            if (sideQuestEntry3 != null)
                sideQuestEntry3.Clear();
        }
    }
}
