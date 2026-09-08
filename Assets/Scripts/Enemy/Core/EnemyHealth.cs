using Duna.QuestSystem;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : HealthController
{
    [SerializeField] private EnemyReward reward;

    [Header("Drops")]
    [SerializeField] private DropItem[] possibleDrops;

    [SerializeField] private Transform dropPoint;

    private EnemyIdentity identity;

    //==============================================================//

    private void Awake()
    {
        reward = GetComponent<EnemyReward>();
        identity = GetComponent<EnemyIdentity>();
    }


    //==============================================================//

    public override void Die()
    {
        if (IsDead.Value)
            return;

        if (IsServer)
        {
            DropRandomItem();

            if (reward != null)
                reward.GiveRewards();

            if (identity != null)
                QuestEvents.RaiseKillEnemy(identity.EnemyID, 1);
        }

        base.Die();
    }


    //==============================================================//

    private void DropRandomItem()
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
            return;

        float totalWeight = 0f;

        foreach (DropItem item in possibleDrops)
        {
            if (item.prefab != null)
                totalWeight += item.weight;
        }

        if (totalWeight <= 0f)
            return;

        float randomValue = Random.Range(0f, totalWeight);

        float currentWeight = 0f;

        foreach (DropItem item in possibleDrops)
        {
            if (item.prefab == null)
                continue;

            currentWeight += item.weight;

            if (randomValue <= currentWeight)
            {
                SpawnDrop(item.prefab);
                return;
            }
        }
    }


    //==============================================================//

    private void SpawnDrop(NetworkObject prefab)
    {
        Vector3 position = dropPoint != null
            ? dropPoint.position
            : transform.position;

        NetworkObject pickup = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        pickup.Spawn();
    }
}
