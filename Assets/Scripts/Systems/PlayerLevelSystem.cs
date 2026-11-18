using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerLevelSystem : NetworkBehaviour
{
    public Action OnXpChanged;

    public NetworkVariable<int> level = new(1);

    public NetworkVariable<float> currentXp = new();

    public NetworkVariable<float> xpToNextLevel = new(100);

    private PlayerContext ctx;

    [Header("Level Up VFX")]
    [SerializeField] private GameObject levelUpVFXPrefab;
    [SerializeField] private Transform levelUpVFXPoint;

    //==================================================//
    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //==================================================//
    public override void OnNetworkSpawn()
    {
        currentXp.OnValueChanged += OnValueChanged;
        xpToNextLevel.OnValueChanged += OnValueChanged;
        level.OnValueChanged += OnLevelChanged;
    }

    //==================================================//
    void OnValueChanged(
        float previous,
        float current)
    {
        OnXpChanged?.Invoke();
    }

    //==================================================//
    void OnLevelChanged(
        int previous,
        int current)
    {
        OnXpChanged?.Invoke();
    }

    //==================================================//
    public void AddXP(float amount)
    {
        if (!IsServer || amount <= 0f)
            return;

        currentXp.Value += amount;

        while (currentXp.Value >= xpToNextLevel.Value)
        {
            currentXp.Value -= xpToNextLevel.Value;

            level.Value++;

            ctx.stats.ApplyLevelBonus(level.Value);

            xpToNextLevel.Value *= 1.25f;

            // Efecto visual de subida de nivel.
            PlayLevelUpVFXRpc();
        }

        OnXpChanged?.Invoke();
    }

    //==================================================//
    public float GetXpNormalized()
    {
        if (xpToNextLevel.Value <= 0)
            return 0;

        return currentXp.Value / xpToNextLevel.Value;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayLevelUpVFXRpc()
    {
        if (levelUpVFXPrefab == null)
            return;

        Vector3 position = levelUpVFXPoint != null
            ? levelUpVFXPoint.position
            : transform.position;

        Quaternion rotation = levelUpVFXPoint != null
            ? levelUpVFXPoint.rotation
            : transform.rotation;

        Instantiate(
            levelUpVFXPrefab,
            position,
            rotation
        );
    }
}
