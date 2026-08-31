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


        [Header("UI")]
        [SerializeField]
        private TextMeshProUGUI questTitle;


        [SerializeField]
        private TextMeshProUGUI questDescription;


        [SerializeField]
        private TextMeshProUGUI objectiveText;

        private bool isBound;


        //==========================================================//
        // BIND
        //==========================================================//

        public void Bind(
            QuestManager manager)
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


            questManager =
                manager;


            SubscribeToEvents();


            isBound =
                true;


            RefreshUI(
                null
            );
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


            isBound =
                false;
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


            if (
                questManager.ActiveQuests.Count
                == 0
            )
            {
                ClearUI();

                return;
            }


            QuestInstance activeQuest =
                questManager.ActiveQuests[0];


            if (activeQuest == null)
            {
                ClearUI();

                return;
            }


            questTitle.text =
                activeQuest.Data.QuestName;


            questDescription.text =
                activeQuest.Data.Description;


            QuestObjectiveRuntime objective =
                activeQuest.CurrentObjective;


            if (objective == null)
            {
                ClearUI();

                return;
            }


            objectiveText.text =
                $"{objective.Data.Description}\n" +
                $"{objective.CurrentAmount}/" +
                $"{objective.Data.RequiredAmount}";
        }


        //==========================================================//
        // CLEAR UI
        //==========================================================//

        private void ClearUI()
        {
            if (questTitle != null)
                questTitle.text =
                    "";


            if (questDescription != null)
                questDescription.text =
                    "";


            if (objectiveText != null)
                objectiveText.text =
                    "";
        }
    }
}
