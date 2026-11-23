using UnityEngine;

[CreateAssetMenu(menuName = "Items/Mana Potion")]
public class ManaPotion : Item
{
    [Header("Mana")]
    public float manaAmount = 20f;

    public override bool CanConsume => true;

    public override void Consume(GameObject user)
    {
        if (user == null)
            return;

        var mana = user.GetComponentInParent<PlayerMana>();

        if (mana == null)
        {
            Debug.LogWarning("No se encontró PlayerMana en el jugador.");
            return;
        }

        mana.RestoreMana(manaAmount);

        Debug.Log("Maná restaurado: " + manaAmount);
    }
}
