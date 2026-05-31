using System;
using System.Globalization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerEquipment : NetworkBehaviour
{
    public Action OnEquipmentChanged;

    //public Weapon weapon;
    //public Armor armor;
    public Weapon weapon { get; private set; }
    public Armor armor { get; private set; }

    private NetworkVariable<FixedString64Bytes> weaponId = new();
    private NetworkVariable<FixedString64Bytes> armorId = new();

    public ItemDatabase itemDatabase;

    [Header("Visual")]
    [SerializeField] private Transform weaponHolder;

    private GameObject currentWeaponInstance;
    private PlayerContext ctx;

    //====================================================//
    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //====================================================//

    public override void OnNetworkSpawn()
    {
        weaponId.OnValueChanged += OnWeaponChanged;
        armorId.OnValueChanged += OnArmorChanged;

        // refresco inicial
        RefreshEquipment();
    }

    //====================================================//

    private void overrideOnDestroy()
    {
        weaponId.OnValueChanged -= OnWeaponChanged;
        armorId.OnValueChanged -= OnArmorChanged;
    }

    //====================================================//

    public void Equip(EquipableItem item)
    {
        if (!IsOwner)
            return;

        EquipServerRpc(item.itemId);
    }

    //====================================================//

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void EquipServerRpc(string itemId)
    {
        Inventory inventory = ctx.inventory;

        // validar que tenga el item
        bool hasItem = false;

        foreach (var slot in inventory.items)
        {
            if (slot.itemId == itemId)
            {
                hasItem = true;
                break;
            }
        }

        if (!hasItem)
            return;

        Item item = itemDatabase.GetByItemId(itemId);

        if (item == null)
            return;

        // sincronizar IDs
        if (item is Weapon)
        {
            weaponId.Value = itemId;
        }
        else if (item is Armor)
        {
            armorId.Value = itemId;
        }
    }

    //====================================================//

    private void OnWeaponChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        weapon = itemDatabase.GetByItemId(current.ToString()) as Weapon;

        RefreshWeaponVisual();

        OnEquipmentChanged?.Invoke();
    }

    //====================================================//

    private void OnArmorChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        armor = itemDatabase.GetByItemId(current.ToString()) as Armor;

        OnEquipmentChanged?.Invoke();
    }

    //====================================================//

    private void RefreshEquipment()
    {
        if (!string.IsNullOrEmpty(weaponId.Value.ToString()))
        {
            weapon = itemDatabase.GetByItemId(weaponId.Value.ToString()) as Weapon;
        }

        if (!string.IsNullOrEmpty(armorId.Value.ToString()))
        {
            armor = itemDatabase.GetByItemId(armorId.Value.ToString()) as Armor;
        }

        RefreshWeaponVisual();

        OnEquipmentChanged?.Invoke();
    }

    //====================================================//

    private void RefreshWeaponVisual()
    {
        if (currentWeaponInstance != null)
            Destroy(currentWeaponInstance);

        if (weapon == null)
            return;

        if (weapon.weaponPrefab != null &&
            weaponHolder != null)
        {
            currentWeaponInstance = Instantiate(weapon.weaponPrefab, weaponHolder);

            currentWeaponInstance.transform.localPosition = Vector3.zero;

            currentWeaponInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
