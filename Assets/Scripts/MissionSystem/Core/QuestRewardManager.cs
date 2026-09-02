using UnityEngine;

namespace Duna.QuestSystem
{
    /// <summary>
    /// Gestiona la entrega de recompensas de las misiones.
    /// </summary>
    public class QuestRewardManager : MonoBehaviour
    {
        [Header("Player Inventory")]
        [SerializeField] private Inventory inventory;

        /// <summary>
        /// Entrega todas las recompensas de una misión.
        /// </summary>
        public void GiveRewards(QuestData quest)
        {
            if (quest == null)
                return;


            foreach (RewardData reward in quest.Rewards)
            {
                GiveReward(reward);
            }
        }


        private void GiveReward(RewardData reward)
        {
            switch (reward.RewardType)
            {
                case RewardType.Experience:

                    GiveExperience(reward.Amount);

                    break;


                case RewardType.Gold:

                    GiveGold(reward.Amount);

                    break;


                case RewardType.Item:

                    GiveItem(
                        reward.ItemID,
                        reward.Amount
                    );

                    break;
            }
        }


        private void GiveExperience(int amount)
        {
            Debug.Log(
                $"Jugador recibe {amount} experiencia."
            );


            // Aquí después conectaremos PlayerStats
        }


        private void GiveGold(int amount)
        {
            Debug.Log(
                $"Jugador recibe {amount} oro."
            );


            // Aquí conectaremos economía si es necesario
        }


        private void GiveItem(string itemID, int amount)
        {
            if (inventory == null)
            {
                Debug.LogError("QuestRewardManager: No hay Inventory asignado.");
                return;
            }

            inventory.AddItem(itemID, amount);

            Debug.Log($"Recompensa entregada: {amount}x {itemID}");
        }
    }
}
