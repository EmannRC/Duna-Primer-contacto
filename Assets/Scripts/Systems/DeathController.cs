using UnityEngine;
using System.Collections;

public class DeathController : MonoBehaviour
{
    [Header("General")]
    public bool disableMovement = false;
    public bool destroyOnDeath = false;
    public float destroyDelay = 3f;

    [Header("References")]
    public AudioSource deathSound;

    private PlayerContext ctx;

    //==================================================//
    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //==================================================//
    void Start()
    {
        if (ctx.health != null)
            ctx.health.OnDeath += Death;
    }

    //==================================================//
    void Death()
    {
        if (disableMovement && ctx.movement != null)
        {
            ctx.movement.IsMovementLocked = true;
            ctx.movement.SetMoveInput(Vector2.zero);
            ctx.movement.SetSprint(false);
            ctx.movement.SetCrouch(false);

            ctx.playerAnimation.PlayDeath();
        }

        if (deathSound != null)
            deathSound.Play();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }

    //==================================================//
    void OnDestroy()
    {
        if (ctx.health != null)
            ctx.health.OnDeath -= Death;
    }
}
