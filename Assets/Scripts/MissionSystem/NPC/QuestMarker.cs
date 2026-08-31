using UnityEngine;

namespace Duna.QuestSystem.UI
{
    /// <summary>
    /// Muestra un marcador visual cuando este objeto
    /// corresponde al objetivo actual de una misión.
    /// </summary>
    public class QuestMarker : MonoBehaviour
    {
        private NPCIdentity npcIdentity;

        [Header("Visual")]
        [SerializeField]
        private GameObject markerVisual;


        private QuestManager questManager;


        //================================================//
        // UNITY
        //================================================//

        private void Awake()
        {
            npcIdentity = GetComponent<NPCIdentity>();
        }
        private void Start()
        {
            HideMarker();
        }


        //================================================//
        // INITIALIZE
        //================================================//

        public void Initialize(
            QuestManager manager)
        {
            if (manager == null)
            {
                Debug.LogWarning(
                    $"QuestMarker en '{name}' recibió un QuestManager NULL."
                );

                return;
            }


            questManager = manager;


            SubscribeToEvents();


            RefreshMarker();
        }


        //================================================//
        // EVENTS
        //================================================//

        private void SubscribeToEvents()
        {
            if (questManager == null)
                return;


            questManager.OnQuestAccepted += HandleQuestChanged;
            questManager.OnObjectiveUpdated += HandleQuestChanged;
            questManager.OnQuestCompleted += HandleQuestChanged;
            questManager.OnQuestTurnedIn += HandleQuestChanged;
        }


        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }


        private void UnsubscribeFromEvents()
        {
            if (questManager == null)
                return;


            questManager.OnQuestAccepted -= HandleQuestChanged;
            questManager.OnObjectiveUpdated -= HandleQuestChanged;
            questManager.OnQuestCompleted -= HandleQuestChanged;
            questManager.OnQuestTurnedIn -= HandleQuestChanged;
        }


        private void HandleQuestChanged(
            QuestInstance quest)
        {
            RefreshMarker();
        }


        //================================================//
        // REFRESH
        //================================================//

        private void RefreshMarker()
        {
            if (questManager == null)
            {
                HideMarker();
                return;
            }


            bool shouldShow = false;


            foreach (QuestInstance quest in questManager.ActiveQuests)
            {
                QuestObjectiveRuntime objective =
                    quest.CurrentObjective;


                if (objective == null)
                    continue;


                if (objective.Data.TargetID == npcIdentity.NPCID)
                {
                    shouldShow = true;
                    break;
                }
            }


            if (shouldShow)
                ShowMarker();
            else
                HideMarker();
        }


        //================================================//
        // SHOW / HIDE
        //================================================//

        private void ShowMarker()
        {
            if (markerVisual != null)
                markerVisual.SetActive(true);
        }


        private void HideMarker()
        {
            if (markerVisual != null)
                markerVisual.SetActive(false);
        }
    }
}
