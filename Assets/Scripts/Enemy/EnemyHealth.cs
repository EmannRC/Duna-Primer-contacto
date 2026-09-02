using Duna.QuestSystem;
using UnityEngine;

public class EnemyHealth : HealthController
{
    [SerializeField] private EnemyReward reward;

    private EnemyIdentity identity;

    private void Awake()
    {
        reward = GetComponent<EnemyReward>();

        identity = GetComponent<EnemyIdentity>();
    }

    public override void Die()
    {
        reward.GiveRewards();

        QuestEvents.RaiseKillEnemy(identity.EnemyID, 1);

        base.Die();

        Destroy(gameObject);
    }
}
