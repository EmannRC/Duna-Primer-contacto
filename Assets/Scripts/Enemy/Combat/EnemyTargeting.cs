using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyTargeting : NetworkBehaviour
{
    [Header("Targeting")]
    [SerializeField, Min(0f)] private float detectionRange = 15f;

    [Header("Gizmos")]
    [SerializeField] private bool showDetectionRange = true;
    [SerializeField] private Color detectionColor = Color.yellow;
    [SerializeField] private Color targetColor = Color.red;

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
    private void OnDrawGizmosSelected()
    {
        if (!showDetectionRange)
            return;

        // Radio de detección.
        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Línea hacia el objetivo actual.
        if (CurrentTarget != null)
        {
            Gizmos.color = targetColor;
            Gizmos.DrawLine(
                transform.position,
                CurrentTarget.position
            );

            Gizmos.DrawWireSphere(
                CurrentTarget.position,
                0.4f
            );
        }
    }
}
