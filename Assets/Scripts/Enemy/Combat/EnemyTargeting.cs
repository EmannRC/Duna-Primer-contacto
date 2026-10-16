using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyTargeting : NetworkBehaviour
{
    [Header("Targeting")]
    [SerializeField, Min(0f)] private float detectionRange = 15f;

    public Transform CurrentTarget { get; private set; }

    private void Update()
    {
        if (!IsServer)
            return;

        FindPlayer();
    }

    private void FindPlayer()
    {
        float closestDistance = detectionRange * detectionRange;

        CurrentTarget = null;

        foreach (var player in FindObjectsByType<PlayerContext>(FindObjectsSortMode.None))
        {
            if (player == null ||
                (player.TryGetComponent<PlayerHealth>(out PlayerHealth health) && health.IsDead.Value))
                continue;

            float distance = (transform.position - player.transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                CurrentTarget = player.transform;
            }
        }
    }
}
