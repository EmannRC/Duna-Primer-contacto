using System.Collections;
using UnityEngine;

public class PlayerHealth : HealthController
{
    public override void Die()
    {
        base.Die();

        GameManager.Instance.SetState(GameState.PlayerDead);
    }
}
