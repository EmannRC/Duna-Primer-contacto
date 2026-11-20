using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using UnityEngine;

namespace Duna.QuestSystem
{
    public class QuestManager : NetworkBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Database")]
        [SerializeField] private QuestDatabase questDatabase;
        [SerializeField] private string startingQuestID;

        private readonly List<QuestInstance> activeQuests = new();
        private readonly List<QuestInstance> completedQuests = new();

        public IReadOnlyList<QuestInstance> ActiveQuests => activeQuests;
        public IReadOnlyList<QuestInstance> CompletedQuests => completedQuests;

        // Eventos locales (se disparan en Host y Clients para actualizar UI y recompensas)
        public event Action<QuestInstance> OnQuestAccepted;
        public event Action<QuestInstance> OnQuestCompleted;
        public event Action<QuestInstance> OnQuestTurnedIn;
        public event Action<QuestInstance> OnQuestAbandoned;
        public event Action<QuestInstance> OnObjectiveUpdated;
        public event Action<QuestInstance, int, QuestObjectiveRuntime> OnObjectiveCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (!string.IsNullOrWhiteSpace(startingQuestID))
                {
                    AcceptQuest(startingQuestID);
                }
            }
            else
            {
                // El cliente pide las misiones activas justo cuando su QuestManager ya existe en escena
                RequestFullSyncServerRpc();
            }
        }

        public override void OnNetworkDespawn()
        {
            // Ya no dependemos de OnClientConnectedCallback
        }



        //================================================//
        // LATE-JOIN SYNC (Jugador entra con partida empezada)
        //================================================//

        private void HandleClientConnected(ulong clientId)
        {
            if (!IsServer || clientId == NetworkManager.ServerClientId)
                return;

            List<QuestSyncData> syncList = new();

            foreach (var q in activeQuests)
                syncList.Add(CreateSyncData(q));

            foreach (var q in completedQuests)
                syncList.Add(CreateSyncData(q));

            // Nuevo RpcTarget en NGO 2.x para enviar a un cliente específico
            SyncAllQuestsRpc(syncList.ToArray(), RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        private QuestSyncData CreateSyncData(QuestInstance q)
        {
            return new QuestSyncData
            {
                QuestID = q.Data.QuestID,
                CurrentObjectiveIndex = q.CurrentObjectiveIndex,
                CurrentObjectiveAmount = q.CurrentObjective != null ? q.CurrentObjective.CurrentAmount : 0,
                State = q.State
            };
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SyncAllQuestsRpc(QuestSyncData[] syncedQuests, RpcParams rpcParams = default)
        {
            if (IsServer) return;

            activeQuests.Clear();
            completedQuests.Clear();

            foreach (var sync in syncedQuests)
            {
                QuestData data = questDatabase.GetQuest(sync.QuestID.ToString());
                if (data == null) continue;

                QuestInstance instance = new QuestInstance(data);
                instance.ApplyNetworkState(sync.CurrentObjectiveIndex, sync.CurrentObjectiveAmount, sync.State);

                if (sync.State == QuestState.TurnedIn)
                    completedQuests.Add(instance);
                else
                    activeQuests.Add(instance);
            }

            // Notificar a la UI que las misiones iniciales llegaron
            OnQuestAccepted?.Invoke(null);
        }

        //================================================//
        // ACCEPT QUEST
        //================================================//

        public void RequestAcceptQuest(string questID)
        {
            if (IsServer)
                AcceptQuest(questID);
            else
                AcceptQuestServerRpc(questID);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void AcceptQuestServerRpc(string questID)
        {
            AcceptQuest(questID);
        }

        public bool AcceptQuest(string questID)
        {
            if (!IsServer)
            {
                RequestAcceptQuest(questID);
                return false;
            }

            if (questDatabase == null || string.IsNullOrWhiteSpace(questID)) return false;

            QuestData questData = questDatabase.GetQuest(questID);
            if (questData == null || HasQuest(questID)) return false;
            if (!questData.Repeatable && IsQuestCompleted(questID)) return false;

            QuestInstance quest = new QuestInstance(questData);
            activeQuests.Add(quest);

            // SendTo.NotServer evita tener que poner "if (IsServer) return;" dentro del RPC
            AcceptQuestClientsRpc(questID);
            OnQuestAccepted?.Invoke(quest);

            if (quest.IsCompleted)
            {
                OnQuestCompleted?.Invoke(quest);
                if (quest.Data.AutoComplete)
                    TurnInQuest(quest.Data.QuestID);
            }

            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void AcceptQuestClientsRpc(string questID)
        {
            QuestData questData = questDatabase.GetQuest(questID);
            if (questData == null || HasQuest(questID)) return;

            QuestInstance quest = new QuestInstance(questData);
            activeQuests.Add(quest);
            OnQuestAccepted?.Invoke(quest);
        }

        //================================================//
        // PROGRESS OBJECTIVE (Solo el Server lo llama)
        //================================================//

        public void ReportProgress(ObjectiveType type, string targetID, int amount = 1)
        {
            if (!IsServer) return;

            for (int i = activeQuests.Count - 1; i >= 0; i--)
            {
                QuestInstance quest = activeQuests[i];
                if (quest.TryProgress(type, targetID, amount))
                {
                    int completedIndex = quest.LastCompletedObjectiveIndex;
                    int newAmount = quest.CurrentObjective != null ? quest.CurrentObjective.CurrentAmount : 0;

                    SyncProgressClientsRpc(
                        quest.Data.QuestID,
                        quest.CurrentObjectiveIndex,
                        newAmount,
                        quest.State,
                        completedIndex
                    );

                    NotifyObjectiveUpdated(quest, completedIndex);
                }
            }
        }

        [Rpc(SendTo.NotServer)]
        private void SyncProgressClientsRpc(
            string questID,
            int objectiveIndex,
            int currentAmount,
            QuestState newState,
            int completedObjectiveIndex)
        {
            QuestInstance quest = GetActiveQuest(questID);

            // Auto-crear la misión si el cliente aún no la tenía por timing de red
            if (quest == null)
            {
                QuestData questData = questDatabase.GetQuest(questID);
                if (questData == null) return;

                quest = new QuestInstance(questData);
                activeQuests.Add(quest);
            }

            quest.ApplyNetworkState(objectiveIndex, currentAmount, newState);
            NotifyObjectiveUpdated(quest, completedObjectiveIndex);
        }

        //================================================//
        // TURN IN QUEST
        //================================================//

        public void RequestTurnInQuest(string questID)
        {
            if (IsServer)
                TurnInQuest(questID);
            else
                TurnInQuestServerRpc(questID);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void TurnInQuestServerRpc(string questID)
        {
            TurnInQuest(questID);
        }

        public bool TurnInQuest(string questID)
        {
            if (!IsServer)
            {
                RequestTurnInQuest(questID);
                return false;
            }

            QuestInstance quest = GetActiveQuest(questID);
            if (quest == null || !quest.IsCompleted) return false;

            quest.TurnIn();
            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            TurnInQuestClientsRpc(questID);
            OnQuestTurnedIn?.Invoke(quest);

            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void TurnInQuestClientsRpc(string questID)
        {
            QuestInstance quest = GetActiveQuest(questID);
            if (quest == null) return;

            quest.TurnIn();
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            OnQuestTurnedIn?.Invoke(quest);
        }

        //================================================//
        // ABANDON & QUERIES
        //================================================//

        public bool AbandonQuest(string questID)
        {
            if (!IsServer)
            {
                AbandonQuestServerRpc(questID);
                return false;
            }

            QuestInstance quest = GetActiveQuest(questID);
            if (quest == null) return false;

            activeQuests.Remove(quest);
            AbandonQuestClientsRpc(questID);
            OnQuestAbandoned?.Invoke(quest);
            return true;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void AbandonQuestServerRpc(string questID) => AbandonQuest(questID);

        [Rpc(SendTo.NotServer)]
        private void AbandonQuestClientsRpc(string questID)
        {
            QuestInstance quest = GetActiveQuest(questID);
            if (quest == null) return;

            activeQuests.Remove(quest);
            OnQuestAbandoned?.Invoke(quest);
        }

        public bool HasQuest(string questID) => GetActiveQuest(questID) != null;
        public bool IsQuestCompleted(string questID) => GetCompletedQuest(questID) != null;

        public QuestInstance GetActiveQuest(string questID) =>
            activeQuests.Find(q => q.Data.QuestID == questID);

        public QuestInstance GetCompletedQuest(string questID) =>
            completedQuests.Find(q => q.Data.QuestID == questID);

        public void NotifyObjectiveUpdated(QuestInstance quest, int completedObjectiveIndex = -1)
        {
            if (quest == null) return;

            if (completedObjectiveIndex >= 0 && completedObjectiveIndex < quest.Objectives.Count)
            {
                OnObjectiveCompleted?.Invoke(quest, completedObjectiveIndex, quest.Objectives[completedObjectiveIndex]);
            }

            OnObjectiveUpdated?.Invoke(quest);

            if (quest.State == QuestState.Completed)
            {
                OnQuestCompleted?.Invoke(quest);

                if (IsServer && quest.Data.AutoComplete)
                {
                    TurnInQuest(quest.Data.QuestID);
                }
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestFullSyncServerRpc(RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            List<QuestSyncData> syncList = new();

            foreach (var q in activeQuests)
                syncList.Add(CreateSyncData(q));

            foreach (var q in completedQuests)
                syncList.Add(CreateSyncData(q));

            SyncAllQuestsRpc(syncList.ToArray(), RpcTarget.Single(senderId, RpcTargetUse.Temp));
        }

        //================================================//
        // SHARED DIALOGUE REWARDS
        //================================================//

        public void RequestSharedDialogueReward(string itemID, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemID) || amount <= 0)
                return;

            if (IsServer)
            {
                DistributeDialogueRewardToLocalPlayer(itemID, amount);
                GrantDialogueRewardClientsRpc(itemID, amount);
            }
            else
            {
                RequestDialogueRewardServerRpc(itemID, amount);
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestDialogueRewardServerRpc(string itemID, int amount)
        {
            DistributeDialogueRewardToLocalPlayer(itemID, amount);
            GrantDialogueRewardClientsRpc(itemID, amount);
        }

        [Rpc(SendTo.NotServer)]
        private void GrantDialogueRewardClientsRpc(string itemID, int amount)
        {
            DistributeDialogueRewardToLocalPlayer(itemID, amount);
        }

        private void DistributeDialogueRewardToLocalPlayer(string itemID, int amount)
        {
            if (LocalPlayerBootstrap.Instance != null && LocalPlayerBootstrap.Instance.LocalPlayer != null)
            {
                Inventory localInv = LocalPlayerBootstrap.Instance.LocalPlayer.GetComponentInChildren<Inventory>();
                if (localInv != null)
                {
                    localInv.RequestAddItem(itemID, amount);
                }
            }
        }
    }
}
