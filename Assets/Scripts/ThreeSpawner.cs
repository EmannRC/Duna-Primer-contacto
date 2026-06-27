using System.Collections.Generic;
using UnityEngine;

public class ThreeSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Terrain terrain;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] treePrefabs;

    [Header("Generación")]
    [SerializeField] private int treeCount = 500;
    [SerializeField] private float minDistance = 3f;

    [Header("Escala")]
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.3f;

    private readonly List<Vector3> spawnedPositions = new();

    [ContextMenu("Spawn Trees")]
    private void SpawnTrees()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain no asignado.");
            return;
        }

        if (treePrefabs.Length == 0)
        {
            Debug.LogError("No hay prefabs asignados.");
            return;
        }

        spawnedPositions.Clear();

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = treeCount * 20;

        while (spawned < treeCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(0, terrainData.size.x);
            float z = Random.Range(0, terrainData.size.z);

            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;

            Vector3 spawnPosition = new Vector3(
                x + terrainPos.x,
                y,
                z + terrainPos.z);

            bool validPosition = true;

            foreach (Vector3 pos in spawnedPositions)
            {
                if (Vector3.Distance(pos, spawnPosition) < minDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            if (!validPosition)
                continue;

            GameObject prefab =
                treePrefabs[Random.Range(0, treePrefabs.Length)];

            GameObject tree = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.Euler(0, Random.Range(0, 360), 0),
                transform);

            float scale = Random.Range(minScale, maxScale);
            tree.transform.localScale *= scale;

            spawnedPositions.Add(spawnPosition);
            spawned++;
        }

        Debug.Log($"Árboles generados: {spawned}");
    }

    [ContextMenu("Clear Trees")]
    private void ClearTrees()
    {
        while (transform.childCount > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(0).gameObject);
#else
            Destroy(transform.GetChild(0).gameObject);
#endif
        }
    }
}
