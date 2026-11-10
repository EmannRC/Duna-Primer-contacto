using System;
using System.Globalization;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerEquipment : NetworkBehaviour
{
    public Action OnEquipmentChanged;

   
    public Weapon weapon { get; private set; }
    public Armor armor { get; private set; }

    private NetworkVariable<FixedString64Bytes> weaponId = new();
    private NetworkVariable<FixedString64Bytes> armorId = new();

    public ItemDatabase itemDatabase;

    [Header("Visual")]
    [SerializeField] private Transform weaponHolder;

    public Transform CurrentFirePoint => currentWeapon != null ? currentWeapon.FirePoint : null;

    private GameObject currentWeaponInstance;
    private WeaponInstance currentWeapon;
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

        if (inventory == null)
            return;

        // Comprobar que el objeto esté en el inventario.
        if (!inventory.HasItem(itemId, 1))
            return;

        Item item = itemDatabase.GetByItemId(itemId);

        if (item == null)
            return;

        string previousItemId = string.Empty;

        // Obtener el objeto actualmente equipado
        // de la misma categoría.
        if (item is Weapon)
        {
            previousItemId = weaponId.Value.ToString();
        }
        else if (item is Armor)
        {
            previousItemId = armorId.Value.ToString();
        }
        else
        {
            return;
        }

        // Retirar el objeto nuevo del inventario.
        inventory.RemoveItem(itemId, 1);

        // Devolver el objeto anterior al inventario.
        if (!string.IsNullOrEmpty(previousItemId))
        {
            bool returned = inventory.TryAddItem(previousItemId, 1);

            // Si no se pudo devolver el anterior,
            // restaurar el nuevo y cancelar el intercambio.
            if (!returned)
            {
                inventory.TryAddItem(itemId, 1);
                return;
            }
        }

        // Actualizar el equipamiento.
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

        currentWeaponInstance = null;
        currentWeapon = null;

        if (weapon == null)
            return;

        if (weapon.weaponPrefab != null && weaponHolder != null)
        {
            currentWeaponInstance =
                Instantiate(weapon.weaponPrefab, weaponHolder);

            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;

            currentWeapon =
                currentWeaponInstance.GetComponent<WeaponInstance>();

            if (currentWeapon == null)
            {
                Debug.LogError(
                    $"El prefab del arma {weapon.name} no tiene WeaponInstance."
                );
            }
        }
    }
}
