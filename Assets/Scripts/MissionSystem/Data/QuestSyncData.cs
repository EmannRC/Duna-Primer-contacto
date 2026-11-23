using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Duna.QuestSystem
{
    public struct QuestSyncData : INetworkSerializable
    {
        public FixedString64Bytes QuestID;
        public int CurrentObjectiveIndex;
        public int CurrentObjectiveAmount;
        public QuestState State;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref QuestID);
            serializer.SerializeValue(ref CurrentObjectiveIndex);
            serializer.SerializeValue(ref CurrentObjectiveAmount);
            serializer.SerializeValue(ref State);
        }
    }
}
