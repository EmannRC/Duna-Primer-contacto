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

        [Header("Trigger Settings")]
        [SerializeField] private string playerTag = "Player";

        [Header("Respawn Settings")]
        [SerializeField, Min(0f)]
        private float respawnDelay = 10f;

        [SerializeField, Min(0.1f)]
        private float checkInterval = 0.5f;

        private readonly List<EnemySpawnSlot> spawnSlots = new();
        private readonly List<Coroutine> respawnRoutines = new();

        private bool initialized;

        private void OnTriggerEnter(Collider other)
        {
            // Solo el servidor puede activar el spawner.
            if (!IsServer || initialized)
                return;

            // Busca el NetworkObject del jugador.
            NetworkObject playerNetworkObject =
                other.GetComponentInParent<NetworkObject>();

            if (playerNetworkObject == null)
                return;

            // Comprueba que el objeto detectado sea un jugador.
            if (!playerNetworkObject.CompareTag(playerTag))
                return;

            SpawnEnemies();

            Debug.Log(
                $"[EnemySpawner] Activado por {playerNetworkObject.name}.",
                this
            );
        }

        public void SpawnEnemies()
        {
            if (!IsServer || initialized)
                return;

            initialized = true;

            if (enemies == null || spawnPoints == null)
            {
                Debug.LogError(
                    "[EnemySpawner] Faltan referencias en el Inspector.",
                    this
                );
                return;
            }

            int spawnIndex = 0;

            foreach (EnemySpawnData enemyData in enemies)
            {
                if (enemyData.enemyPrefab == null ||
                    enemyData.amount <= 0)
                {
                    continue;
                }

                for (int i = 0; i < enemyData.amount; i++)
                {
                    if (spawnIndex >= spawnPoints.Length)
                    {
                        Debug.LogWarning(
                            "[EnemySpawner] No hay suficientes Spawn Points.",
                            this
                        );

                        return;
                    }

                    Transform spawnPoint = spawnPoints[spawnIndex];
                    spawnIndex++;

                    if (spawnPoint == null)
                    {
                        Debug.LogWarning(
                            "[EnemySpawner] Hay un Spawn Point sin asignar.",
                            this
                        );

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
                }
            }
        }

        private IEnumerator ManageEnemyRespawn(EnemySpawnSlot slot)
        {
            // Genera el enemigo inicial.
            SpawnEnemy(slot);

            while (true)
            {
                // Espera hasta que el enemigo muera o desaparezca.
                while (slot.currentEnemy != null &&
                       slot.currentEnemy.IsSpawned)
                {
                    yield return new WaitForSeconds(checkInterval);
                }

                // Espera antes de regenerarlo.
                yield return new WaitForSeconds(respawnDelay);

                if (IsServer)
                {
                    SpawnEnemy(slot);
                }
                else
                {
                    yield break;
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

            Debug.Log(
                $"[EnemySpawner] Enemigo generado: {enemy.name}",
                this
            );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            foreach (Coroutine routine in respawnRoutines)
            {
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
            }

            respawnRoutines.Clear();
        }
    }
}
