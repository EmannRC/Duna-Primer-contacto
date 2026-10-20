using UnityEngine;

namespace Duna.QuestSystem
{
    public class QuestColliderUnlock : MonoBehaviour
    {
        [Header("Quest")]
        [SerializeField] private string requiredQuestID;

        [Header("Wall")]
        [SerializeField] private GameObject wall;

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
                DisableWall();
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

            DisableWall();
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

            DisableWall();
        }

        //================================================//
        // DISABLE COLLIDER
        //================================================//

        private void DisableWall()
        {
            if (wall == null)
            {
                Debug.LogWarning(
                    $"[{name}] No tiene un muro asignado."
                );

                return;
            }

            if (wall.activeInHierarchy)
                return;

            wall.SetActive(false);

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
