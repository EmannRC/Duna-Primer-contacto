using Unity.Netcode;
using UnityEngine;

namespace Duna.QuestSystem
{
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField]
        private NetworkObject enemyPrefab;

        [SerializeField]
        private Transform[] spawnPoints;


        public void SpawnEnemies()
        {
            if (!IsServer)
                return;

            foreach (Transform spawnPoint in spawnPoints)
            {
                NetworkObject enemy =
                    Instantiate(
                        enemyPrefab,
                        spawnPoint.position,
                        spawnPoint.rotation
                    );

                enemy.Spawn();
            }
        }
    }
}
