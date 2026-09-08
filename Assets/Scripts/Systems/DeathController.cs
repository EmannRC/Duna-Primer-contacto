using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DeathController : NetworkBehaviour
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    [Header("Entity")]
    [SerializeField] private EntityType entityType;

    [Header("General")]
    [SerializeField] private bool disableMovement = true;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource deathSound;

    private PlayerContext playerCtx;
    private EnemyContext enemyCtx;

    private bool deathHandled;


    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        if (entityType == EntityType.Player)
            playerCtx = GetComponent<PlayerContext>();
        else
            enemyCtx = GetComponent<EnemyContext>();
    }


    //========================================================//
    // DEATH
    //========================================================//

    public void HandleDeath()
    {
        if (!IsServer)
            return;

        if (deathHandled)
            return;

        deathHandled = true;

        if (entityType == EntityType.Player)
            HandlePlayerDeath();
        else
            HandleEnemyDeath();

        if (deathSound != null)
            deathSound.Play();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }


    //========================================================//
    // PLAYER
    //========================================================//

    private void HandlePlayerDeath()
    {
        if (playerCtx == null)
            return;

        if (disableMovement && playerCtx.movement != null)
            playerCtx.movement.SetMovementLocked(true);

        playerCtx.playerAnimation.PlayDeathAnimation();
    }


    //========================================================//
    // ENEMY
    //========================================================//

    private void HandleEnemyDeath()
    {
        if (enemyCtx == null)
            return;

        if (disableMovement && enemyCtx.movement != null)
            enemyCtx.movement.SetMovementLocked(true);

        enemyCtx.enemyAnimation.PlayDeathAnimation();
    }
}
