using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyTargeting : NetworkBehaviour
{
    [Header("Targeting")]
    [SerializeField, Min(0f)] private float detectionRange = 15f;
    [SerializeField, Min(0.05f)] private float scanInterval = 0.2f;

    [Header("Gizmos")]
    [SerializeField] private bool showDetectionRange = true;
    [SerializeField] private Color detectionColor = Color.yellow;
    [SerializeField] private Color targetColor = Color.red;

    public Transform CurrentTarget { get; private set; }

    private float nextScanTime;

    private void Update()
    {
        if (!IsServer)
            return;

        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        FindPlayer();
    }

    private void FindPlayer()
    {
        float closestDistance = detectionRange * detectionRange;
        PlayerContext closestPlayer = null;

        foreach (PlayerContext player in PlayerRegistry.Players)
        {
            if (player == null)
                continue;

            if (player.TryGetComponent<PlayerHealth>(out PlayerHealth health)
                && health.IsDead.Value)
                continue;

            float distance =
                (transform.position - player.transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        CurrentTarget = closestPlayer != null
            ? closestPlayer.transform
            : null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDetectionRange)
            return;

        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (CurrentTarget != null)
        {
            Gizmos.color = targetColor;
            Gizmos.DrawLine(transform.position, CurrentTarget.position);
            Gizmos.DrawWireSphere(CurrentTarget.position, 0.4f);
        }
    }
}
