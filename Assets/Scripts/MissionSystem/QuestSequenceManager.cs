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
            Debug.Log($"[QuestSequence] Se entregó: {quest.Data.QuestID}");

            foreach (QuestSequence sequence in sequences)
            {
                Debug.Log(
                    $"[QuestSequence] Comparando: " +
                    $"{sequence.completedQuestID} == {quest.Data.QuestID}"
                );

                if (sequence.completedQuestID != quest.Data.QuestID)
                    continue;

                Debug.Log(
                    $"[QuestSequence] ¡Coincide! Intentando activar: " +
                    $"{sequence.nextQuestID}"
                );

                bool result = questManager.AcceptQuest(
                    sequence.nextQuestID
                );

                Debug.Log(
                    $"[QuestSequence] AcceptQuest devolvió: {result}"
                );

                return;
            }

            Debug.LogWarning(
                $"[QuestSequence] No encontré una secuencia para: " +
                $"{quest.Data.QuestID}"
            );
        }
    }
}
