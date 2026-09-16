using TMPro;
using UnityEngine;

namespace Duna.QuestSystem.UI
{
    public class QuestEntryUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private TextMeshProUGUI questTitle;

        [SerializeField]
        private TextMeshProUGUI questDescription;

        [SerializeField]
        private TextMeshProUGUI objectiveText;

        public bool IsEmpty { get; private set; }

        //==========================================================//
        // SET QUEST
        //==========================================================//

        public void SetQuest(QuestInstance quest)
        {
            if (quest == null)
            {
                Clear();
                return;
            }

            IsEmpty = false;

            questTitle.text =
                quest.Data.QuestName;

            questDescription.text =
                quest.Data.Description;

            RefreshObjective(quest);
        }


        //==========================================================//
        // REFRESH OBJECTIVE
        //==========================================================//

        public void RefreshObjective(QuestInstance quest)
        {
            if (quest == null)
            {
                Clear();
                return;
            }

            QuestObjectiveRuntime objective =
                quest.CurrentObjective;

            if (objective == null)
            {
                objectiveText.text = "";
                return;
            }

            objectiveText.text =
                $"{objective.Data.Description}\n" +
                $"{objective.CurrentAmount}/" +
                $"{objective.Data.RequiredAmount}";
        }


        //==========================================================//
        // CLEAR
        //==========================================================//

        public void Clear()
        {
            IsEmpty = true;

            if (questTitle != null)
                questTitle.text = "";

            if (questDescription != null)
                questDescription.text = "";

            if (objectiveText != null)
                objectiveText.text = "";
        }
    }
}
