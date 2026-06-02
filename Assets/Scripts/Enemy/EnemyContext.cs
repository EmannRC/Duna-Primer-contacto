using UnityEngine;

public class EnemyContext : MonoBehaviour
{
    [Header("Core")]
    public EnemyHealth health;
    public EnemyMovement movement;
    public EnemyCombat combat;
    public EnemyTargeting targeting;
    public EnemyFormation formation;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        targeting = GetComponent<EnemyTargeting>();
        formation = GetComponent<EnemyFormation>();
    }
}
