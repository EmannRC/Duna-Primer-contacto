using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

namespace Duna.QuestSystem
{
    public class EnemySpawner : NetworkBehaviour
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            public NetworkObject enemyPrefab;
            public int amount = 1;
        }

        private class EnemySpawnSlot
        {
            public NetworkObject prefab;
            public Transform spawnPoint;
            public NetworkObject currentEnemy;
        }

        [Header("Enemies")]
        [SerializeField] private EnemySpawnData[] enemies;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Respawn Settings")]
        [SerializeField, Min(0f)]
        private float respawnDelay = 10f;

        [SerializeField, Min(0.1f)]
        private float checkInterval = 0.5f;

        private readonly List<EnemySpawnSlot> spawnSlots = new();
        private readonly List<Coroutine> respawnRoutines = new();

        private bool initialized;

        public void SpawnEnemies()
        {
            if (!IsServer || initialized)
                return;

            initialized = true;

            int spawnIndex = 0;

            foreach (EnemySpawnData enemyData in enemies)
            {
                if (enemyData.enemyPrefab == null || enemyData.amount <= 0)
                    continue;

                for (int i = 0; i < enemyData.amount; i++)
                {
                    if (spawnIndex >= spawnPoints.Length)
                    {
                        Debug.LogWarning(
                            "No hay suficientes Spawn Points para todos los enemigos.",
                            this
                        );

                        return;
                    }

                    Transform spawnPoint = spawnPoints[spawnIndex];

                    if (spawnPoint == null)
                    {
                        Debug.LogWarning(
                            $"El Spawn Point {spawnIndex} no está asignado.",
                            this
                        );

                        spawnIndex++;
                        continue;
                    }

                    EnemySpawnSlot slot = new EnemySpawnSlot
                    {
                        prefab = enemyData.enemyPrefab,
                        spawnPoint = spawnPoint
                    };

                    spawnSlots.Add(slot);

                    Coroutine routine = StartCoroutine(
                        ManageEnemyRespawn(slot)
                    );

                    respawnRoutines.Add(routine);

                    spawnIndex++;
                }
            }
        }

        private IEnumerator ManageEnemyRespawn(EnemySpawnSlot slot)
        {
            // Genera el enemigo inicial.
            SpawnEnemy(slot);

            while (true)
            {
                // Espera hasta que el enemigo muera o desaparezca
                // de la sesión de red.
                while (slot.currentEnemy != null &&
                       slot.currentEnemy.IsSpawned)
                {
                    yield return new WaitForSeconds(checkInterval);
                }

                // Espera antes de regenerarlo.
                yield return new WaitForSeconds(respawnDelay);

                // Solo el servidor puede crear enemigos.
                if (IsServer)
                {
                    SpawnEnemy(slot);
                }
            }
        }

        private void SpawnEnemy(EnemySpawnSlot slot)
        {
            if (!IsServer ||
                slot.prefab == null ||
                slot.spawnPoint == null)
            {
                return;
            }

            NetworkObject enemy = Instantiate(
                slot.prefab,
                slot.spawnPoint.position,
                slot.spawnPoint.rotation
            );

            enemy.Spawn();

            slot.currentEnemy = enemy;
        }
    }
}
