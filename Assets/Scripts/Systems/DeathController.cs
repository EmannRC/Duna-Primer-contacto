using UnityEngine;
using System.Collections;

public class DeathController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool disableMovement = true;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource deathSound;

    private IDeathSource deathSource;
    private IDeathMovement deathMovement;
    private IDeathAnimation deathAnimation;

    private bool deathHandled;


    //==============================================================//

    private void Awake()
    {
        deathSource =
            GetComponent<IDeathSource>();

        deathMovement =
            GetComponent<IDeathMovement>();

        deathAnimation =
            GetComponent<IDeathAnimation>();
    }


    //==============================================================//

    private void Start()
    {
        if (deathSource == null)
        {
            Debug.LogError(
                $"{name}: No se encontró un IDeathSource."
            );

            return;
        }

        deathSource.OnDeath += HandleDeath;
    }


    //==============================================================//

    private void HandleDeath()
    {
        if (deathHandled)
            return;

        deathHandled = true;


        // BLOQUEAR MOVIMIENTO
        if (disableMovement &&
            deathMovement != null)
        {
            deathMovement.SetMovementLocked(true);
        }


        // ANIMACIÓN DE MUERTE
        if (deathAnimation != null)
        {
            deathAnimation.PlayDeathAnimation();
        }


        // SONIDO
        if (deathSound != null)
        {
            deathSound.Play();
        }


        // DESTRUCCIÓN
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }


    //==============================================================//

    private void OnDestroy()
    {
        if (deathSource != null)
        {
            deathSource.OnDeath -= HandleDeath;
        }
    }
}
