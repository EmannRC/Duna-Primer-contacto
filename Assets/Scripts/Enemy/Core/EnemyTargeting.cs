using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyTargeting : NetworkBehaviour
{
    public Transform CurrentTarget { get; private set; }

    private void Update()
    {
        if (!IsServer)
            return;

        FindPlayer();
    }

    private void FindPlayer()
    {
        float closestDistance = float.MaxValue;

        CurrentTarget = null;

        foreach (var player in FindObjectsByType<PlayerContext>(FindObjectsSortMode.None))
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                CurrentTarget = player.transform;
            }
        }
    }
}
