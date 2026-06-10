using System.Collections;
using UnityEngine;

public class PlayerHealth : HealthController
{
    public override void Die()
    {
        if (IsDead.Value)
            return;

        base.Die();

        CoopGameManager.Instance.CheckDefeat();
    }
}
