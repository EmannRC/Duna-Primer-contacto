using UnityEngine;

public class EnemyContext : MonoBehaviour
{
    [Header("Core")]
    public EnemyStats stats;
    public EnemyHealth health;
    public GroundEnemyMovement movement;
    public FlyingEnemyMovement flyingMovement;
    
    public EnemyRangeCombat combat;
    public EnemyMeleeCombat meleeCombat;
    public EnemyTargeting targeting;
    public EnemyFormation formation;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<GroundEnemyMovement>();
        flyingMovement = GetComponent<FlyingEnemyMovement>();
        combat = GetComponent<EnemyRangeCombat>();
        meleeCombat = GetComponent<EnemyMeleeCombat>();
        targeting = GetComponent<EnemyTargeting>();
        formation = GetComponent<EnemyFormation>();
        stats = GetComponent<EnemyStats>();
    }
}
