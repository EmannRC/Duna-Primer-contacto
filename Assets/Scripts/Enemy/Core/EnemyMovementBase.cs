using Unity.Netcode;
using UnityEngine;

public abstract class EnemyMovementBase : NetworkBehaviour
{
    public abstract void SetMovementLocked(bool locked);
}
