using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Objects")]
    [SerializeField] private GameObject[] prefabs;

    [Header("Timing")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float spawnInterval = 5f;

    [Header("Limits")]
    [SerializeField] private bool loop = true;
    [SerializeField] private int maxAlive = 5;
    [SerializeField] private int totalSpawnLimit = -1; //infinito

    [Header("Area")]
    [SerializeField] private float spawnRadius = 5f;

    private readonly List<GameObject> aliveObjects = new();

    private Coroutine spawnRoutine;

    private int totalSpawned;

    //================================================//

    private void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
    }

    //================================================//

    public void StartSpawning()
    {
        if (spawnRoutine != null)
            return;

        spawnRoutine =
            StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
            return;

        StopCoroutine(spawnRoutine);

        spawnRoutine = null;
    }

    //================================================//

    IEnumerator SpawnRoutine()
    {
        if (startDelay > 0)
        {
            yield return
                new WaitForSeconds(startDelay);
        }

        while (true)
        {
            CleanupDeadReferences();

            if (CanSpawn())
            {
                Spawn();
            }

            if (!loop)
                break;

            yield return
                new WaitForSeconds(spawnInterval);
        }

        spawnRoutine = null;
    }

    //================================================//

    bool CanSpawn()
    {
        if (prefabs.Length == 0)
            return false;

        if (aliveObjects.Count >= maxAlive)
            return false;

        if (
            totalSpawnLimit != -1 &&
            totalSpawned >= totalSpawnLimit
        )
            return false;

        return true;
    }

    //================================================//

    void Spawn()
    {
        GameObject prefab =
            prefabs[
                Random.Range(
                    0,
                    prefabs.Length
                )
            ];

        Vector3 spawnPosition =
            GetRandomPosition();

        GameObject spawned =
            Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

        aliveObjects.Add(spawned);

        totalSpawned++;
    }

    //================================================//

    Vector3 GetRandomPosition()
    {
        Vector2 random =
            Random.insideUnitCircle
            * spawnRadius;

        return transform.position +
               new Vector3(
                   random.x,
                   0,
                   random.y
               );
    }

    //================================================//

    void CleanupDeadReferences()
    {
        aliveObjects.RemoveAll(
            obj => obj == null
        );
    }

    //================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            spawnRadius
        );
    }
}
