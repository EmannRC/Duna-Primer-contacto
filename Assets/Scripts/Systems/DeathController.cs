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

    private bool deathHandled;

    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    void Start()
    {
        ctx.health.OnDeath += HandleDeath;
    }

    void HandleDeath()
    {
        if (deathHandled) return;
        deathHandled = true;

        //  bloqueo local inmediato
        if (ctx.movement != null)
        {
            ctx.movement.IsMovementLocked = true;
            ctx.movement.SetMoveInput(Vector2.zero);
        }

        //  animación (solo una vez)
        ctx.animationSync.Dead.Value = true;

        if (deathSound)
            deathSound.Play();

        Destroy(gameObject, destroyDelay);
    }

    void OnDestroy()
    {
        if (ctx?.health != null)
            ctx.health.OnDeath -= HandleDeath;
    }
}
