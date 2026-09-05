using Unity.Netcode;
using UnityEngine;

namespace Duna.QuestSystem
{
    public class EnemySpawner : NetworkBehaviour
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            public NetworkObject enemyPrefab;
            public int amount;
        }

        [SerializeField]
        private EnemySpawnData[] enemies;

        [SerializeField]
        private Transform[] spawnPoints;


        public void SpawnEnemies()
        {
            if (!IsServer)
                return;

            int spawnIndex = 0;

            foreach (EnemySpawnData enemyData in enemies)
            {
                for (int i = 0; i < enemyData.amount; i++)
                {
                    // Si no quedan puntos disponibles, dejamos de spawnear
                    if (spawnIndex >= spawnPoints.Length)
                    {
                        Debug.LogWarning("No hay suficientes Spawn Points.");
                        return;
                    }

                    Transform spawnPoint = spawnPoints[spawnIndex];

                    NetworkObject enemy = Instantiate(
                        enemyData.enemyPrefab,
                        spawnPoint.position,
                        spawnPoint.rotation
                    );

                    enemy.Spawn();

                    spawnIndex++;
                }
            }
        }
    }
}
