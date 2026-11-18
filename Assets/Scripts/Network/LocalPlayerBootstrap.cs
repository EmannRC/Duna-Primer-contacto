using Duna.QuestSystem;
using Duna.QuestSystem.UI;
using Duna.UI;
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
    //[SerializeField] private Transform crosshair;
    [SerializeField] private CrosshairUI crosshairUI;
    [SerializeField] private QuestTrackerUI questTrackerUI;
    [SerializeField] private UI_Manager deathMenu;
    [SerializeField] private MenuButtonUI menuButtonUI;
    [SerializeField] private PlayerSkillUI skillUI;

    public PlayerContext LocalPlayer { get; private set; }

    //==========================================================//
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }


        Instance = this;
    }


    //==========================================================//
    public void Setup(Transform playerRoot)
    {
        Debug.Log("LOCAL PLAYER SETUP");


        PlayerContext ctx = playerRoot.GetComponent<PlayerContext>();


        NetworkObject net = playerRoot.GetComponent<NetworkObject>();


       // QuestManager questManager =
            //ctx.questManager;


        if (ctx == null || net == null)
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

        LocalPlayer = ctx;

        // Enlazar la cámara primero: la vista local no debe depender de que
        // los sistemas de estado y misiones estén inicializados.
        if (mainCamera == null || cameraController == null)
        {
            Debug.LogError(
                "[LocalPlayerBootstrap] Falta asignar Main Camera o CameraController."
            );
        }
        else
        {
            cameraController.SetTarget(playerRoot);
            ctx.mainCamera = mainCamera;

            if (ctx.movement != null)
                ctx.movement.SetCameraTransform(mainCamera.transform);
            else
                Debug.LogError("[LocalPlayerBootstrap] PlayerMovement no está asignado.");

            if (ctx.targeting != null)
                ctx.targeting.Bind(cameraController);
            else
                Debug.LogError("[LocalPlayerBootstrap] TargetingSystem no está asignado.");
        }

        if (ctx.state == null)
            Debug.LogError("[LocalPlayerBootstrap] PlayerState no está asignado.");

        if (questManager == null)
            Debug.LogError("[LocalPlayerBootstrap] El Player no tiene QuestManager.");

        //=========================
        // QUEST OBJECTIVE EVENTS
        //=========================

        if (questManager != null)
        {
            QuestObjectiveCompletionEvent[] objectiveEvents =
                FindObjectsByType<QuestObjectiveCompletionEvent>(FindObjectsSortMode.None);

            foreach (QuestObjectiveCompletionEvent objectiveEvent in objectiveEvents)
                objectiveEvent.Initialize(questManager);
        }


        //=========================
        // CAMERA
        //=========================

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


        if (questTrackerUI != null && questManager != null)
            questTrackerUI.Bind(questManager);

        //=========================
        // SKILL UI
        //=========================

        if (skillUI == null)
        {
            Debug.LogError(
                "[LocalPlayerBootstrap] SkillUI no está asignado en el Inspector."
            );
        }
        else if (ctx.skillSystem == null)
        {
            Debug.LogError(
                "[LocalPlayerBootstrap] PlayerContext no tiene PlayerSkillSystem asignado."
            );
        }
        else
        {
            skillUI.Bind(ctx.skillSystem);
        }

        //=========================
        // RESPAWN
        //=========================
        PlayerRespawn playerRespawn = playerRoot.GetComponent<PlayerRespawn>();
        if (playerRespawn == null)
        {
            Debug.LogError(
                "[LocalPlayerBootstrap] El Player local no tiene PlayerRespawn."
            );
        }
        else if (menuButtonUI == null)
        {
            Debug.LogError(
                "[LocalPlayerBootstrap] MenuButtonUI no está asignado en el Inspector."
            );
        }
        else
        {
            menuButtonUI.Bind(playerRespawn);

            Debug.Log(
                "[LocalPlayerBootstrap] PlayerRespawn vinculado correctamente al MenuButtonUI."
            );
        }


        //=========================
        // CROSSHAIR
        //=========================

        Debug.Log($"[Crosshair] UI: {crosshairUI}");
        Debug.Log($"[Crosshair] Targeting: {ctx.targeting}");

        if (crosshairUI == null)
        {
            Debug.LogError(
                "[Crosshair] LocalPlayerBootstrap no tiene CrosshairUI asignado."
            );

            return;
        }

        if (ctx.targeting == null)
        {
            Debug.LogError(
                "[Crosshair] PlayerContext no tiene TargetingSystem asignado."
            );

            return;
        }

        crosshairUI.Bind(ctx.targeting);

        //=========================
        // NPC Marker
        //=========================

        if (questManager == null)
            return;

        QuestMarker[] markers =
            FindObjectsByType<QuestMarker>(FindObjectsSortMode.None);

        foreach (QuestMarker marker in markers)
        {
            marker.Initialize(questManager);
        }
    }

    public void ShowDeathMenu()
    {
        if (deathMenu != null)
            deathMenu.ShowDefeatMenu();
    }
    public void HideDeathMenu()
    {
        if (deathMenu != null)
            deathMenu.HideDefeatMenu();
    }

}
