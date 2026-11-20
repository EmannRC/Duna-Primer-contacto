using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Escucha los eventos del juego y actualiza automáticamente
    /// el progreso de todas las misiones activas.
    /// </summary>
    public class QuestTracker : NetworkBehaviour
    {
        [SerializeField] private QuestManager questManager;

        private void Awake()
        {
            if (questManager == null)
                questManager = QuestManager.Instance;
        }

        public override void OnNetworkSpawn()
        {
            if (questManager == null)
                questManager = QuestManager.Instance;

            // Eventos de interacción/UI (pueden ocurrir en cualquier cliente)
            QuestEvents.OnTalkToNPC += OnTalkToNPC;
            QuestEvents.OnDeliverItem += OnDeliverItem;

            // Eventos autoritativos del mundo (solo el servidor los escucha para evitar duplicados)
            if (IsServer)
            {
                QuestEvents.OnCollectItem += OnCollectItem;
                QuestEvents.OnKillEnemy += OnKillEnemy;
            }
        }

        public override void OnNetworkDespawn()
        {
            QuestEvents.OnTalkToNPC -= OnTalkToNPC;
            QuestEvents.OnDeliverItem -= OnDeliverItem;

            if (IsServer)
            {
                QuestEvents.OnCollectItem -= OnCollectItem;
                QuestEvents.OnKillEnemy -= OnKillEnemy;
            }
        }

        //------------------------------------------------//

        private void OnTalkToNPC(string npcID)
        {
            SendProgressToServer(ObjectiveType.Talk, npcID, 1);
        }

        private void OnDeliverItem(string itemID, int amount)
        {
            SendProgressToServer(ObjectiveType.Deliver, itemID, amount);
        }

        private void OnCollectItem(string itemID, int amount)
        {
            SendProgressToServer(ObjectiveType.Collect, itemID, amount);
        }

        private void OnKillEnemy(string enemyID, int amount)
        {
            Debug.Log($"[QuestTracker] Kill recibido en Server: {enemyID} | Amount: {amount}");
            SendProgressToServer(ObjectiveType.Kill, enemyID, amount);
        }

        //------------------------------------------------//

        private void SendProgressToServer(ObjectiveType type, string targetID, int amount)
        {
            if (questManager == null || string.IsNullOrWhiteSpace(targetID) || amount <= 0)
                return;

            if (IsServer)
            {
                questManager.ReportProgress(type, targetID, amount);
            }
            else
            {
                ReportProgressServerRpc(type, targetID, amount);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void ReportProgressServerRpc(ObjectiveType type, string targetID, int amount)
        {
            if (questManager != null)
            {
                questManager.ReportProgress(type, targetID, amount);
            }
        }
    }
}
