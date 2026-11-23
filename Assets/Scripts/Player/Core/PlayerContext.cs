using Duna.InteractionSystem;
using Duna.Player;
using Duna.QuestSystem;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerContext : NetworkBehaviour
{
    private static readonly HashSet<PlayerContext> _activePlayers = new();

    public static IReadOnlyCollection<PlayerContext> ActivePlayers
    {
        get
        {
            _activePlayers.RemoveWhere(p => p == null);
            return _activePlayers;
        }
    }

    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Overhead Name UI")]
    [SerializeField] private TMP_Text overheadNameText;
    [SerializeField] private bool hideOwnOverheadName = true;

    [Header("State")]
    public PlayerState state;

    [Header("Core")]
    public PlayerStatsManager stats;
    public PlayerMana mana;
    public PlayerEquipment equipment;
    public Inventory inventory;
    public InventoryItemActions actions;
    public PlayerCrafting crafting;
    public TargetingSystem targeting;
    public PlayerInputEvents inputEvents;
    public PlayerLevelSystem levelSystem;
    public PlayerInteraction interaction;
    public QuestManager questManager;

    [Header("Health")]
    public PlayerHealth health;

    [Header("Movement")]
    public CharacterController controller;
    public PlayerMovement movement;

    [Header("Combat")]
    public PlayerCombat combat;
    public ShootController shooter;
    public PlayerSkillSystem skillSystem;

    [Header("Visual / UI")]
    public Animator animator;
    public PlayerAnimation playerAnimation;
    public CraftingUI craftingUI;
    public Camera mainCamera;

    private Camera localCamera;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            _activePlayers.Add(this);

        PlayerName.OnValueChanged += OnPlayerNameChanged;

        // Ocultar nuestro propio nombre sobre nuestra cabeza si está activado
        if (IsOwner && hideOwnOverheadName && overheadNameText != null)
        {
            overheadNameText.gameObject.SetActive(false);
        }

        // Aplicar el valor actual (importante para clientes que entran tarde o al cargar escena)
        UpdateNameUI(PlayerName.Value.ToString());

        // El dueño envía su nombre guardado en PlayerProfile al servidor
        if (IsOwner)
        {
            SubmitPlayerNameRpc(new FixedString64Bytes(PlayerProfile.PlayerName));
        }
    }

    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnPlayerNameChanged;
        _activePlayers.Remove(this);
    }

    private void LateUpdate()
    {
        // Hacer que el texto mire siempre hacia la cámara del jugador local (Billboard)
        if (overheadNameText == null || !overheadNameText.gameObject.activeInHierarchy)
            return;

        if (localCamera == null)
            localCamera = Camera.main;

        if (localCamera != null)
        {
            overheadNameText.transform.rotation = localCamera.transform.rotation;
        }
    }

    private void OnPlayerNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        UpdateNameUI(newName.ToString());
    }

    private void UpdateNameUI(string newName)
    {
        if (overheadNameText != null)
            overheadNameText.text = newName;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void SubmitPlayerNameRpc(FixedString64Bytes playerName)
    {
        string validatedName = playerName.ToString().Trim();

        if (string.IsNullOrWhiteSpace(validatedName))
            validatedName = "Jugador";

        if (validatedName.Length > 8)
            validatedName = validatedName.Substring(0, 8);

        PlayerName.Value = new FixedString64Bytes(validatedName);
    }
}

