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

    [Header("Visual / UI")]
    public PlayerAnimationSync animationSync;
    public Transform crosshair;
    public CraftingUI craftingUI;
    public Camera mainCamera;

}

