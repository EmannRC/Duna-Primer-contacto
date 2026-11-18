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

    [Header("Death VFX")]
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private Transform deathVFXPoint;

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

        PlayDeathVFXClientRpc();

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

        // Destruir enemigos para todos los jugadores.
        if (destroyOnDeath && entityType == EntityType.Enemy)
        {
            StartCoroutine(DestroyEnemyAfterDelay());
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

    [Rpc(SendTo.ClientsAndHost)]
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

        if (enemyCtx.movement != null)
        {
            // Activar comportamiento específico de muerte.
            enemyCtx.movement.OnDeath();

            // Bloquear movimiento normal.
            if (disableMovement)
            {
                enemyCtx.movement.SetMovementLocked(true);
            }
        }

        if (enemyCtx.enemyAnimation != null)
        {
            enemyCtx.enemyAnimation.PlayDeathAnimation();
        }
    }

    private IEnumerator DestroyEnemyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (!IsServer)
            yield break;

        NetworkObject networkObject = GetComponent<NetworkObject>();

        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn(true);
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

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayDeathVFXClientRpc()
    {
        if (deathVFXPrefab == null)
            return;

        Vector3 position = deathVFXPoint != null
            ? deathVFXPoint.position
            : transform.position;

        Instantiate(deathVFXPrefab, position, Quaternion.identity);
    }
}
