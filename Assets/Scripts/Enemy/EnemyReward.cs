using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    [Header("XP")]
    public int xpReward = 25;

    [SerializeField] private PlayerLevelSystem playerLevel;

    private void Update()
    {
        if (playerLevel == null)
        {
            playerLevel =
                FindFirstObjectByType<PlayerLevelSystem>();
        }
    }

    public void GiveRewards()
    {
        if (playerLevel != null)
        {
            playerLevel.AddXP(xpReward);

            Debug.Log("XP otorgada: " + xpReward);
        }
    }
}
