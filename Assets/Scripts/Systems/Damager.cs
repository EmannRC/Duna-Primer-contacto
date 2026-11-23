using Unity.Netcode;
using UnityEngine;

public class Damager : MonoBehaviour
{
    [SerializeField] private float damage = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(damage);
    }

}
