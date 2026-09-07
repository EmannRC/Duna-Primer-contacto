using UnityEngine;

namespace Duna.QuestSystem
{
    public class QuestColliderUnlock : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private string requiredQuestID;

        [Header("Collider")]
        [SerializeField] private Collider targetCollider;

        private QuestManager questManager;

        //================================================//
        // START
        //================================================//

        private void Start()
        {
            FindQuestManager();
        }

        //================================================//
        // FIND QUEST MANAGER
        //================================================//

        private void FindQuestManager()
        {
            QuestManager[] managers =
                FindObjectsByType<QuestManager>(
                    FindObjectsSortMode.None
                );

            foreach (QuestManager manager in managers)
            {
                // Nos quedamos con el QuestManager que
                // tenga la misión que necesitamos.
                if (manager.HasQuest(requiredQuestID) ||
                    manager.IsQuestCompleted(requiredQuestID))
                {
                    questManager = manager;
                    Subscribe();
                    CheckIfAlreadyCompleted();
                    return;
                }
            }

            // Si todavía no encontramos el QuestManager,
            // puede que el jugador todavía no haya sido creado.
            Invoke(
                nameof(FindQuestManager),
                0.5f
            );
        }

        //================================================//
        // SUBSCRIBE
        //================================================//

        private void Subscribe()
        {
            if (questManager == null)
                return;

            questManager.OnQuestCompleted +=
                HandleQuestCompleted;

            questManager.OnQuestTurnedIn +=
                HandleQuestTurnedIn;
        }

        //================================================//
        // CHECK ALREADY COMPLETED
        //================================================//

        private void CheckIfAlreadyCompleted()
        {
            if (questManager.IsQuestCompleted(requiredQuestID))
            {
                DisableCollider();
            }
        }

        //================================================//
        // QUEST COMPLETED
        //================================================//

        private void HandleQuestCompleted(
            QuestInstance quest)
        {
            if (quest == null)
                return;

            if (quest.Data.QuestID != requiredQuestID)
                return;

            DisableCollider();
        }

        //================================================//
        // QUEST TURNED IN
        //================================================//

        private void HandleQuestTurnedIn(
            QuestInstance quest)
        {
            if (quest == null)
                return;

            if (quest.Data.QuestID != requiredQuestID)
                return;

            DisableCollider();
        }

        //================================================//
        // DISABLE COLLIDER
        //================================================//

        private void DisableCollider()
        {
            if (targetCollider == null)
            {
                Debug.LogWarning(
                    $"[{name}] No tiene un Collider asignado."
                );

                return;
            }

            if (!targetCollider.enabled)
                return;

            targetCollider.enabled = false;

            Debug.Log(
                $"Quest '{requiredQuestID}' completada. " +
                $"Collider desactivado en '{name}'."
            );
        }

        //================================================//
        // CLEANUP
        //================================================//

        private void OnDestroy()
        {
            CancelInvoke();

            if (questManager != null)
            {
                questManager.OnQuestCompleted -=
                    HandleQuestCompleted;

                questManager.OnQuestTurnedIn -=
                    HandleQuestTurnedIn;
            }
        }
    }
}
