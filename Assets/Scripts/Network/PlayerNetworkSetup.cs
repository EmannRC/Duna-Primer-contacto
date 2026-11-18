using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerNetworkSetup : NetworkBehaviour
{
    private PlayerInput playerInput;
    private CharacterController characterController;
    private PlayerMovement playerMovement; // O el nombre de tu script de movimiento

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        // Congelar movimiento y gravedad al nacer en TODOS los clientes
        if (characterController != null) characterController.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;

        if (!IsOwner)
        {
            if (playerInput != null)
                playerInput.enabled = false;
            return;
        }

        if (playerInput != null)
            playerInput.enabled = true;

        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        while (LocalPlayerBootstrap.Instance == null)
            yield return null;

        // Esperar un FixedUpdate extra asegura que NetworkTransform sincronizó la posición del servidor
        yield return new WaitForFixedUpdate();

        LocalPlayerBootstrap.Instance.Setup(transform.root);

        // Ahora sí, activar colisiones y gravedad solo para el dueño
        if (characterController != null) characterController.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
    }
}
