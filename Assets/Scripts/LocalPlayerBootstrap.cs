using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerBootstrap : MonoBehaviour
{
    public static LocalPlayerBootstrap Instance;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private PlayerHUD hud;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CraftingUI craftingUI;
    [SerializeField] private Transform crosshair;

    //==========================================================//
    void Awake()
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
        var ctx = playerRoot.GetComponent<PlayerContext>();
        var net = playerRoot.GetComponent<NetworkObject>();

        if (ctx == null || net == null) return;
        if (!net.IsOwner) return;

        // CAMERA + INPUT
        cameraController.SetTarget(playerRoot);
        ctx.mainCamera = mainCamera;

        ctx.targeting.Bind(cameraController);
        ctx.inputEvents.Bind(ctx);

        // UI
        hud.Bind(playerRoot);

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

        ctx.crosshair = crosshair;
    }

}
