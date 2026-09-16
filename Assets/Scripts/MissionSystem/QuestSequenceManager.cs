using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    [Serializable]
    public class QuestSequence
    {
        public string completedQuestID;
        public string nextQuestID;
    }


    public class QuestSequenceManager : MonoBehaviour
    {
        [SerializeField]
        private QuestManager questManager;

        [SerializeField]
        private List<QuestSequence> sequences = new();


        private void OnEnable()
        {
            if (questManager != null)
            {
                questManager.OnQuestTurnedIn +=
                    HandleQuestTurnedIn;
            }
        }


        private void OnDisable()
        {
            if (questManager != null)
            {
                questManager.OnQuestTurnedIn -=
                    HandleQuestTurnedIn;
            }
        }


        private void HandleQuestTurnedIn(QuestInstance quest)
        {
            string completedID = quest.Data.QuestID;

            Debug.Log(
                $"[QuestSequence] Completada: {completedID}"
            );

            foreach (QuestSequence sequence in sequences)
            {
                if (sequence.completedQuestID != completedID)
                    continue;

                Debug.Log(
                    $"[QuestSequence] " +
                    $"{completedID}  {sequence.nextQuestID}"
                );

                if (questManager.HasQuest(sequence.nextQuestID))
                {
                    Debug.LogWarning(
                        $"[QuestSequence] " +
                        $"'{sequence.nextQuestID}' ya está activa."
                    );

                    return;
                }

                if (questManager.IsQuestCompleted(sequence.nextQuestID))
                {
                    Debug.LogWarning(
                        $"[QuestSequence] " +
                        $"'{sequence.nextQuestID}' ya fue completada."
                    );

                    return;
                }

                bool accepted =
                    questManager.AcceptQuest(
                        sequence.nextQuestID
                    );

                if (accepted)
                {
                    Debug.Log(
                        $"[QuestSequence] " +
                        $"'{sequence.nextQuestID}' desbloqueada."
                    );
                }
                else
                {
                    Debug.LogError(
                        $"[QuestSequence]  " +
                        $"No se pudo aceptar '{sequence.nextQuestID}'."
                    );
                }

                return;
            }

            Debug.LogWarning(
                $"[QuestSequence] No hay una secuencia " +
                $"configurada para '{completedID}'."
            );
        }
    }
}
