using UnityEngine;

namespace Duna.QuestSystem
{
    public enum RewardType
    {
        Experience,
        Gold,
        Item
    }

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
