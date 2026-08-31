using Duna.QuestSystem;
using Duna.QuestSystem.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerBootstrap : MonoBehaviour
{
    public static LocalPlayerBootstrap Instance;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private PlayerHUD hud;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CraftingUI craftingUI;
    [SerializeField] private Transform crosshair;
    [SerializeField] private QuestTrackerUI questTrackerUI;

    //==========================================================//
    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);

            return;
        }


        Instance = this;
    }


    //==========================================================//
    public void Setup(
        Transform playerRoot)
    {
        Debug.Log(
            "LOCAL PLAYER SETUP"
        );


        PlayerContext ctx =
            playerRoot.GetComponent<PlayerContext>();


        NetworkObject net =
            playerRoot.GetComponent<NetworkObject>();


       // QuestManager questManager =
            //ctx.questManager;


        if (
            ctx == null ||
            net == null
        )
        {
            Debug.LogError(
                "El Player no tiene PlayerContext " +
                "o NetworkObject."
            );

            return;
        }

        QuestManager questManager =
            ctx.questManager;


        if (!net.IsOwner)
            return;


        if (questManager == null)
        {
            Debug.LogError(
                "El Player no tiene QuestManager."
            );

            return;
        }


        //=========================
        // CAMERA
        //=========================

        cameraController.SetTarget(
            playerRoot
        );


        ctx.movement.SetCameraTransform(
            mainCamera.transform
        );

        // PlayerRotation usa esta referencia para que el personaje mire hacia
        // donde apunta la cámara mientras se mantiene el botón de apuntado.
        ctx.mainCamera = mainCamera;


        ctx.targeting.Bind(
            cameraController
        );


        //=========================
        // INPUT
        //=========================

        ctx.inputEvents.Bind(
            ctx
        );

        ctx.interaction.Bind(
            ctx
        );


        //=========================
        // UI
        //=========================

        hud.Bind(
            playerRoot
        );


        inventoryUI.Bind(
            ctx.inventory,
            ctx.stats,
            ctx.equipment,
            ctx
        );


        craftingUI.Bind(
            ctx.inventory,
            ctx.crafting
        );


        questTrackerUI.Bind(
            questManager
        );


        //=========================
        // CROSSHAIR
        //=========================

        ctx.crosshair =
            crosshair;

        //=========================
        // NPC Marker
        //=========================

        QuestMarker[] markers =
        FindObjectsByType<QuestMarker>(
            FindObjectsSortMode.None
        );

        foreach (QuestMarker marker in markers)
        {
            marker.Initialize(questManager);
        }
    }

}
