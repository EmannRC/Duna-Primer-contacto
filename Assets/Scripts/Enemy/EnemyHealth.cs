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

    private void Awake()
    {
        reward = GetComponent<EnemyReward>();

        identity = GetComponent<EnemyIdentity>();
    }

    public override void Die()
    {
        if (IsServer)
        {
            DropRandomItem();
        }

        reward.GiveRewards();

        QuestEvents.RaiseKillEnemy(identity.EnemyID, 1);

        base.Die();

        Destroy(gameObject);
    }

    private void DropRandomItem()
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
            return;

        float totalWeight = 0;

        foreach (DropItem item in possibleDrops)
        {
            if (item.prefab != null)
                totalWeight += item.weight;
        }

        float randomValue = Random.Range(0f, totalWeight);

        float currentWeight = 0;

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
