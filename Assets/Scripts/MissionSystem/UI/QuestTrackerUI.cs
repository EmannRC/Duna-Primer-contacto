using TMPro;
using UnityEngine;

namespace Duna.QuestSystem.UI
{
    /// <summary>
    /// Muestra la misión activa actual en pantalla.
    /// </summary>
    public class QuestTrackerUI : MonoBehaviour
    {
        [Header("UI References")]

        [SerializeField]
        private TextMeshProUGUI questTitle;

        [SerializeField]
        private TextMeshProUGUI questDescription;

        [SerializeField]
        private TextMeshProUGUI objectiveText;


        private QuestManager questManager;


        private void Start()
        {
            questManager =
                FindFirstObjectByType<QuestManager>();


            if (questManager == null)
            {
                Debug.LogError(
                    "No existe QuestManager en escena."
                );

                return;
            }

            questManager.OnObjectiveUpdated += RefreshUI;

            questManager.OnQuestAccepted += RefreshUI;

            questManager.OnQuestCompleted += RefreshUI;

            questManager.OnQuestTurnedIn += RefreshUI;


            RefreshUI(null);
        }


        private void OnDestroy()
        {
            if (questManager == null)
                return;

            questManager.OnObjectiveUpdated -= RefreshUI;

            questManager.OnQuestAccepted -= RefreshUI;

            questManager.OnQuestCompleted -= RefreshUI;

            questManager.OnQuestTurnedIn -= RefreshUI;
        }


        private void RefreshUI(QuestInstance quest)
        {
            UpdateCurrentQuest();
        }


        private void UpdateCurrentQuest()
        {
            if (questManager.ActiveQuests.Count == 0)
            {
                ClearUI();
                return;
            }


            QuestInstance quest =
                questManager.ActiveQuests[0];


            questTitle.text =
                quest.Data.QuestName;


            questDescription.text =
                quest.Data.Description;


            if (quest.CurrentObjective != null)
            {
                var objective =
                    quest.CurrentObjective;


                objectiveText.text =
                    $"{objective.Data.Description}\n" +
                    $"{objective.CurrentAmount}/" +
                    $"{objective.Data.RequiredAmount}";
            }
        }


        private void ClearUI()
        {
            questTitle.text = "";

            questDescription.text = "";

            objectiveText.text = "";
        }
    }
}
