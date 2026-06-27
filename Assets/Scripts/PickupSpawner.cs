using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PickupSpawner : NetworkBehaviour
{
    [Header("Pickup")]
    [SerializeField] private NetworkObject pickupPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Respawn Time")]
    [SerializeField] private float respawnTime = 10f;

    private NetworkObject[] activePickups;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        activePickups = new NetworkObject[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnPickup(i);
        }
    }

    //======================================================//

    private void SpawnPickup(int index)
    {
        if (activePickups[index] != null)
            return; 

        NetworkObject pickup =
            Instantiate(pickupPrefab,
                spawnPoints[index].position,
                spawnPoints[index].rotation);

        pickup.Spawn();

        activePickups[index] = pickup;
    }

    //======================================================//

    public void OnPickupCollected(NetworkObject pickup)
    {
        if (!IsServer)
            return;

        int index = GetIndexOfPickup(pickup);

        if (index == -1)
            return;

        activePickups[index] = null;

        StartCoroutine(Respawn(index));
    }

    //======================================================//

    private IEnumerator Respawn(int index)
    {
        yield return new WaitForSeconds(respawnTime);

        SpawnPickup(index);
    }

    //======================================================//

    private int GetIndexOfPickup(NetworkObject pickup)
    {
        for (int i = 0; i < activePickups.Length; i++)
        {
            if (activePickups[i] == pickup)
                return i;
        }

        return -1;
    }
}
