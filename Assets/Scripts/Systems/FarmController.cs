using Unity.Netcode;
using UnityEngine;

public class FarmController : NetworkBehaviour
{
    public enum ResourceState
    {
        Available,
        Cooldown
    }

    [Header("Resource Settings")]
    [SerializeField] private string resourceId = "wood";
    [SerializeField] private int maxHits = 3;

    [Header("Pickup Settings")]
    [SerializeField] private NetworkObject pickupPrefab;
    [SerializeField] private int minDropAmount = 1;
    [SerializeField] private int maxDropAmount = 2;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropRadius = 0.5f;

    [Header("Cooldown")]
    [SerializeField] private float respawnTime = 120f;

    [Header("Visuals")]
    [SerializeField] private GameObject availableVisual;
    [SerializeField] private GameObject depletedVisual;
    [SerializeField] private ParticleSystem hitVFX;
    [SerializeField] private ParticleSystem depletedVFX;

    private readonly NetworkVariable<int> currentHits =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private readonly NetworkVariable<ResourceState> currentState =
        new NetworkVariable<ResourceState>(
            ResourceState.Available,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private readonly NetworkVariable<double> respawnAt =
        new NetworkVariable<double>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public ResourceState State => currentState.Value;

    public int CurrentHits => currentHits.Value;

    public int MaxHits => maxHits;

    public double RespawnAt => respawnAt.Value;

    public float CooldownDuration => respawnTime;

    //================================================//
    // NETWORK
    //================================================//

    public override void OnNetworkSpawn()
    {
        currentState.OnValueChanged += OnStateChanged;

        UpdateVisuals(currentState.Value);

        if (IsServer)
        {
            currentHits.Value = 0;
        }
    }

    public override void OnNetworkDespawn()
    {
        currentState.OnValueChanged -= OnStateChanged;
    }

    //================================================//
    // GATHERING
    //================================================//

    public void TryGather()
    {
        if (!IsSpawned)
            return;

        if (IsServer)
        {
            GatherServer();
        }
        else
        {
            RequestGatherServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestGatherServerRpc(RpcParams rpcParams = default)
    {
        // Validar que el jugador exista.
        if (!NetworkManager.ConnectedClients.TryGetValue(
            rpcParams.Receive.SenderClientId,
            out NetworkClient client))
            return;

        if (client.PlayerObject == null)
            return;

        // Validar distancia.
        float distance = Vector3.Distance(
            client.PlayerObject.transform.position,
            transform.position
        );

        if (distance > 4f)
            return;

        GatherServer();
    }

    private void GatherServer()
    {
        if (!IsServer)
            return;

        if (currentState.Value != ResourceState.Available)
            return;

        currentHits.Value++;

        PlayHitVFXClientRpc();

        SpawnPickups();

        if (currentHits.Value >= maxHits)
        {
            DepleteResource();
        }
    }

    //================================================//
    // PICKUPS
    //================================================//

    private void SpawnPickups()
    {
        if (pickupPrefab == null)
            return;

        int amount = Random.Range(
            minDropAmount,
            maxDropAmount + 1
        );

        Vector3 origin = dropPoint != null
            ? dropPoint.position
            : transform.position;

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;

            Vector3 spawnPosition = origin + new Vector3(
                randomOffset.x,
                0f,
                randomOffset.y
            );

            NetworkObject pickup = Instantiate(
                pickupPrefab,
                spawnPosition,
                Quaternion.identity
            );

            pickup.Spawn();
        }
    }

    //================================================//
    // DEPLETION
    //================================================//

    private void DepleteResource()
    {
        currentState.Value = ResourceState.Cooldown;

        respawnAt.Value =
            NetworkManager.ServerTime.Time + respawnTime;

        PlayDepletedVFXClientRpc();
    }

    //================================================//
    // COOLDOWN
    //================================================//

    private void Update()
    {
        if (!IsServer || !IsSpawned)
            return;

        if (currentState.Value != ResourceState.Cooldown)
            return;

        if (NetworkManager.ServerTime.Time >= respawnAt.Value)
        {
            RespawnResource();
        }
    }

    private void RespawnResource()
    {
        currentHits.Value = 0;
        respawnAt.Value = 0;

        currentState.Value = ResourceState.Available;
    }

    //================================================//
    // VISUALS
    //================================================//

    private void OnStateChanged(
        ResourceState previous,
        ResourceState current)
    {
        UpdateVisuals(current);
    }

    private void UpdateVisuals(ResourceState state)
    {
        bool available = state == ResourceState.Available;

        if (availableVisual != null)
            availableVisual.SetActive(available);

        if (depletedVisual != null)
            depletedVisual.SetActive(!available);
    }

    //================================================//
    // EFFECTS
    //================================================//

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayHitVFXClientRpc()
    {
        if (hitVFX != null)
            hitVFX.Play();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayDepletedVFXClientRpc()
    {
        if (depletedVFX != null)
            depletedVFX.Play();
    }
}

