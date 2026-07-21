using UnityEngine;

namespace Duna.QuestSystem
{
    public interface IQuestItemProvider
    {
        bool HasItem(string itemID, int amount);

        bool TryRemoveItem(string itemID, int amount);
    }
}
