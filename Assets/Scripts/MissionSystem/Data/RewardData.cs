using UnityEngine;

namespace Duna.QuestSystem
{
    [System.Serializable]
    public class RewardData
    {
        [SerializeField] private RewardType rewardType;

        [SerializeField] private int amount;

        [SerializeField] private string itemID;

        public RewardType RewardType => rewardType;
        public int Amount => amount;
        public string ItemID => itemID;
    }
}
