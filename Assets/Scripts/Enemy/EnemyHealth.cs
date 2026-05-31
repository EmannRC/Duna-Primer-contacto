using UnityEngine;

public class EnemyHealth : HealthController
{
    [SerializeField] private EnemyReward reward;

    private void Awake()
    {
        reward = GetComponent<EnemyReward>();
    }

    public override void Die()
    {
        reward.GiveRewards();

        base.Die();

        Destroy(gameObject);
    }
}
