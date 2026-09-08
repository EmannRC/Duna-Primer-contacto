using Unity.Netcode;
using UnityEngine;

public abstract class EnemyMovementBase : NetworkBehaviour, IDeathMovement
{
    public abstract void SetMovementLocked(bool locked);
}
