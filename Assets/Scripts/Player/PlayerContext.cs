using System.Globalization;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    [Header("Core")]
    public PlayerStatsManager stats;
    public PlayerMana mana;
    public PlayerEquipment equipment;
    public Inventory inventory;
    public PlayerActions actions;
    public PlayerCrafting crafting;
    public TargetingSystem targeting;
    public PlayerInputEvents inputEvents;
    public PlayerLevelSystem levelSystem;


    [Header("Health")]
    public PlayerHealth health;

    [Header("Movement")]
    public PlayerMovement movement;
    public PlayerRotation rotation;
    public PlayerAnimation playerAnimation;

    [Header("Combat")]
    public ShootController shooter;

    [Header("Visual / UI refs")]
    public PlayerAnimationSync animationSync;
    public Transform crosshair;
    [SerializeField] private CraftingUI craftingUI;
    [SerializeField] public GameObject mainCamera;


    void Awake()
    {
        Cache();
    }

    void Cache()
    {
        stats = GetComponentInChildren<PlayerStatsManager>();
        mana = GetComponentInChildren<PlayerMana>();
        equipment = GetComponentInChildren<PlayerEquipment>();
        inventory = GetComponentInChildren<Inventory>();
        actions = GetComponentInChildren<PlayerActions>();
        crafting = GetComponentInChildren<PlayerCrafting>();
        targeting = GetComponentInChildren<TargetingSystem>();
        inputEvents = GetComponentInChildren<PlayerInputEvents>();
        levelSystem = GetComponentInChildren<PlayerLevelSystem>();

        movement = GetComponent<PlayerMovement>();
        rotation = GetComponent<PlayerRotation>();
        health = GetComponentInChildren<PlayerHealth>();
        playerAnimation = GetComponentInChildren<PlayerAnimation>();
        animationSync = GetComponentInChildren<PlayerAnimationSync>();

        shooter = GetComponentInChildren<ShootController>();
    }
}

