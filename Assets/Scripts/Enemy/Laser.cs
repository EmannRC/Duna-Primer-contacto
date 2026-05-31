using UnityEngine;

public class Laser : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthController playerHealth = other.GetComponent<HealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
