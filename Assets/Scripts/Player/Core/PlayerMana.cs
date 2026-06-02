using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMana : NetworkBehaviour
{
    public Action OnManaChanged;

    public float baseMana = 100f;
    public float regenRate = 15f;
    public float regenDelay = 1f;
    public float MaxMana { get; private set; }

    public NetworkVariable<float> CurrentMana = new();

    private float lastUseTime;

    private PlayerContext ctx;

    //===========================================================//
    //===========================================================//
    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    public override void OnNetworkSpawn()
    {
        CurrentMana.OnValueChanged += OnManaValueChanged;

        if (IsServer)
        {
            RefreshStats();
        }
    }

    void Start()
    {
        ctx.stats.OnStatsChanged += RefreshStats;
    }

    //===========================================================//
    void OnManaValueChanged(float previous, float current)
    {
        OnManaChanged?.Invoke();
    }

    //===========================================================//
    void RefreshStats()
    {
        MaxMana = baseMana + ctx.stats.GetStat(StatType.Mana);

        if (!IsServer)
            return;

        if (CurrentMana.Value <= 0)
            CurrentMana.Value = MaxMana;

        CurrentMana.Value = Mathf.Min(CurrentMana.Value, MaxMana);
    }

    //===========================================================//
    void Update()
    {
        if (!IsServer)
            return;

        Regenerate();
    }

    //===========================================================//
    public bool TryUse(float amount)
    {
        if (!IsServer)
            return false;

        if (CurrentMana.Value < amount)
            return false;

        CurrentMana.Value -= amount;

        lastUseTime = Time.time;

        return true;
    }

    //===========================================================//
    void Regenerate()
    {
        if (Time.time < lastUseTime + regenDelay)
            return;

        if (CurrentMana.Value >= MaxMana)
            return;

        CurrentMana.Value = Mathf.Min(CurrentMana.Value + regenRate * Time.deltaTime, MaxMana);
    }

    //===========================================================//
    public float GetPercent()
    {
        if (MaxMana <= 0)
            return 0;

        return CurrentMana.Value / MaxMana;
    }
}
