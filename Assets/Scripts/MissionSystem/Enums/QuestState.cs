using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Estado general de una misión.
    /// </summary>
    public enum QuestState
    {
        Locked,
        Available,
        Active,
        Completed,
        TurnedIn,
        Failed
    }
}
