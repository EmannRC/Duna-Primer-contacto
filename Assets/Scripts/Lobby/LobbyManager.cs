using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Serializable]
    public struct PlayerSlotUI
    {
        public TMP_Text playerNameText;
        public Image portrait;
    }

    [Header("Network")]
    [SerializeField] private NetworkObject lobbyPlayerPrefab;

    [Header("Lobby UI Slots")]
    [SerializeField] private PlayerSlotUI[] slots;

    private readonly List<LobbyPlayer> _players = new List<LobbyPlayer>();

    private void Awake()
    {
        Instance = this;
        RefreshUI();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        // Asegura que el Host (o clientes ya conectados antes del spawn) reciban su LobbyPlayer
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            SpawnLobbyPlayerForClient(clientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        SpawnLobbyPlayerForClient(clientId);
    }

    private void SpawnLobbyPlayerForClient(ulong clientId)
    {
        if (_players.Exists(p => p != null && p.OwnerClientId == clientId))
            return;

        NetworkObject lobbyPlayer = Instantiate(lobbyPlayerPrefab);
        lobbyPlayer.SpawnWithOwnership(clientId, destroyWithScene: true);
    }

    //========================================================//
    // REGISTRO Y ACTUALIZACIÓN DE UI POR EVENTOS (SIN UPDATE)
    //========================================================//

    public void RegisterPlayer(LobbyPlayer player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
            _players.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            RefreshUI();
        }
    }

    public void UnregisterPlayer(LobbyPlayer player)
    {
        if (_players.Remove(player))
            RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].playerNameText == null)
                continue;

            if (i < _players.Count && _players[i] != null)
            {
                string displayName = _players[i].PlayerName.Value.ToString();
                slots[i].playerNameText.text = string.IsNullOrEmpty(displayName) ? "Conectando..." : displayName;
            }
            else
            {
                slots[i].playerNameText.text = "Libre";
            }
        }
    }
}
