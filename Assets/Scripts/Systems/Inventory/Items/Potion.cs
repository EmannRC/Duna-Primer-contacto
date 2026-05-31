using UnityEngine;

[CreateAssetMenu(menuName = "Items/Potion")]
public class Potion : Item
{
    public float healAmount = 20f;

    public override bool CanConsume => true;

    public override void Consume(GameObject user)
    {
        var health = user.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.Heal(healAmount);
            Debug.Log("Curado " + healAmount);
        }
    }
}
