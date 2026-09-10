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
        // La muerte siempre la controla el servidor.
        if (!IsServer)
            return;

        if (deathHandled)
            return;

        deathHandled = true;

        if (entityType == EntityType.Player)
        {
            HandlePlayerDeath();
        }
        else
        {
            HandleEnemyDeath();
        }

        if (deathSound != null)
            deathSound.Play();

        // Los Players NO se destruyen.
        // Los enemigos sí pueden destruirse.
        if (destroyOnDeath &&
            entityType == EntityType.Enemy)
        {
            Destroy(gameObject, destroyDelay);
        }
    }


    //========================================================//
    // PLAYER
    //========================================================//

    private void HandlePlayerDeath()
    {
        if (playerCtx == null)
            return;

        ShowPlayerDeathClientRpc();
    }


    //========================================================//
    // PLAYER CLIENT
    //========================================================//

    [ClientRpc]
    private void ShowPlayerDeathClientRpc()
    {
        if (!IsOwner)
            return;

        if (playerCtx == null)
            return;

        // Bloquear movimiento.
        if (disableMovement &&
            playerCtx.movement != null)
        {
            playerCtx.movement.SetMovementLocked(true);
        }

        // Animación de muerte.

        if (playerCtx.playerAnimation != null)
        {
            playerCtx.playerAnimation.PlayDeathAnimation();
        }

        // Mostrar menú de muerte.
        if (LocalPlayerBootstrap.Instance != null)
        {
            LocalPlayerBootstrap.Instance.ShowDeathMenu();
        }
    }


    //========================================================//
    // ENEMY
    //========================================================//

    private void HandleEnemyDeath()
    {
        if (enemyCtx == null)
            return;

        // Bloquear movimiento.
        if (disableMovement &&
            enemyCtx.movement != null)
        {
            enemyCtx.movement.SetMovementLocked(true);
        }

        // Animación de muerte del enemigo.
        if (enemyCtx.enemyAnimation != null)
        {
            enemyCtx.enemyAnimation.PlayDeathAnimation();
        }
    }


    //========================================================//
    // RESET
    //========================================================//

    public void ResetDeathState()
    {
        if (!IsServer)
            return;

        deathHandled = false;
    }
}
