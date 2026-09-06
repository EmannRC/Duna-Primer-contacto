using UnityEngine;

public class WeaponInstance : MonoBehaviour
{
    [SerializeField] private Transform firePoint;

    public Transform FirePoint => firePoint;
}
