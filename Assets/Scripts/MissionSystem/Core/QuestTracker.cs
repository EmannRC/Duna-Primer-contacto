using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Escucha los eventos del juego y actualiza automáticamente
    /// el progreso de todas las misiones activas.
    /// </summary>
    public class QuestTracker : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;

        private void OnEnable()
        {
            QuestEvents.OnTalkToNPC += OnTalkToNPC;
            QuestEvents.OnCollectItem += OnCollectItem;
            QuestEvents.OnDeliverItem += OnDeliverItem;
            QuestEvents.OnKillEnemy += OnKillEnemy;
        }

        private void OnDisable()
        {
            QuestEvents.OnTalkToNPC -= OnTalkToNPC;
            QuestEvents.OnCollectItem -= OnCollectItem;
            QuestEvents.OnDeliverItem -= OnDeliverItem;
            QuestEvents.OnKillEnemy -= OnKillEnemy;
        }

        //------------------------------------------------//

        private void OnTalkToNPC(string npcID)
        {
            UpdateObjectives(ObjectiveType.Talk, npcID, 1);
        }

        private void OnCollectItem(string itemID, int amount)
        {
            UpdateObjectives(ObjectiveType.Collect, itemID, amount);
        }

        private void OnDeliverItem(string itemID, int amount)
        {
            UpdateObjectives(ObjectiveType.Deliver, itemID, amount);
        }

        private void OnKillEnemy(string enemyID, int amount)
        {
            UpdateObjectives(ObjectiveType.Kill, enemyID, amount);
        }

        //------------------------------------------------//

        private void UpdateObjectives(ObjectiveType type, string targetID, int amount)
        {
            if (questManager == null || string.IsNullOrWhiteSpace(targetID) || amount <= 0)
                return;

            // NotifyObjectiveUpdated puede completar y retirar una misión
            // automática. Recorremos una copia para no modificar la
            // colección mientras se la está enumerando.
            QuestInstance[] activeQuests =
                new List<QuestInstance>(questManager.ActiveQuests).ToArray();


            foreach (QuestInstance quest in activeQuests)
            {
                bool progressed =
                    quest.TryProgress(type, targetID, amount);


                if (progressed)
                {
                    questManager.NotifyObjectiveUpdated(quest);
                }
            }
        }
    }
}
