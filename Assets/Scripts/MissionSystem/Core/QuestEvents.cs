using System;
using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Centraliza todos los eventos utilizados por el sistema de misiones.
    /// Ningún sistema conoce directamente al QuestManager.
    /// </summary>
    public static class QuestEvents
    {
        /// <summary>
        /// El jugador habló con un NPC.
        /// </summary>
        public static event Action<string> OnTalkToNPC;

        /// <summary>
        /// El jugador obtuvo un objeto.
        /// </summary>
        public static event Action<string, int> OnCollectItem;

        /// <summary>
        /// El jugador entregó un objeto.
        /// </summary>
        public static event Action<string, int> OnDeliverItem;

        /// <summary>
        /// El jugador derrotó un enemigo.
        /// </summary>
        public static event Action<string, int> OnKillEnemy;

        //------------------------------------------------//

        public static void RaiseTalkToNPC(string npcID)
        {
            OnTalkToNPC?.Invoke(npcID);
        }

        public static void RaiseCollectItem(string itemID, int amount = 1)
        {
            OnCollectItem?.Invoke(itemID, amount);
        }

        public static void RaiseDeliverItem(string itemID, int amount = 1)
        {
            OnDeliverItem?.Invoke(itemID, amount);
        }

        public static void RaiseKillEnemy(string enemyID, int amount = 1)
        {
            OnKillEnemy?.Invoke(enemyID, amount);
        }
    }
}
