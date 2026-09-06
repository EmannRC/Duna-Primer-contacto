using UnityEngine;

public class EnemyIdentity : MonoBehaviour
{
    [SerializeField] private string enemyID;

    public string EnemyID => enemyID;
}
