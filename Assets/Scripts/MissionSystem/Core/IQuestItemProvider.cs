using System.Collections.Generic;
using UnityEngine;

namespace Duna.QuestSystem
{
    public interface IQuestItemProvider
    {
        bool HasItem(string itemID, int amount);

        bool TryRemoveItem(string itemID, int amount);

        bool TryRemoveItems(IReadOnlyList<QuestObjectiveData.RequiredItem> items);
    }
}
