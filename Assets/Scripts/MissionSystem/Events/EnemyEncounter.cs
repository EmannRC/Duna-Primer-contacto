using Unity.Netcode;
using UnityEngine;

public class EnemyEncounter : NetworkBehaviour
{
    [System.Serializable]
    private class EnemySpawn
    {
        public GameObject enemyPrefab;
        public Transform spawnPoint;
    }

    [SerializeField]
    private EnemySpawn[] enemies;


    private bool activated;


    public void ActivateEnemies()
    {
        if (IsServer)
        {
            SpawnEnemiesOnServer();
        }
        else
        {
            ActivateEnemiesServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ActivateEnemiesServerRpc()
    {
        SpawnEnemiesOnServer();
    }

    private void SpawnEnemiesOnServer()
    {
        if (!IsServer || activated)
            return;

        activated = true;

        foreach (EnemySpawn spawn in enemies)
        {
            if (spawn.enemyPrefab == null || spawn.spawnPoint == null)
                continue;

            GameObject enemy = Instantiate(
                spawn.enemyPrefab,
                spawn.spawnPoint.position,
                spawn.spawnPoint.rotation
            );

            NetworkObject networkObject = enemy.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError($"El prefab {spawn.enemyPrefab.name} no tiene NetworkObject.");
                Destroy(enemy);
                continue;
            }

            networkObject.Spawn();
        }
    }
}
