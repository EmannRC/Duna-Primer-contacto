using UnityEngine;

public class EnemyContext : MonoBehaviour
{
    [Header("Core")]
    public EnemyStats stats;
    public EnemyHealth health;

    [Header("Movement")]
    public GroundEnemyMovement groundMovement;
    public FlyingEnemyMovement flyingMovement;
    public EnemyMovementBase movement { get; private set; }

    public EnemyRangeCombat rangedCombat;
    public EnemyMeleeCombat meleeCombat;
    public EnemyTargeting targeting;
    public EnemyFormation formation;

    public Animator animator;
    public EnemyAnimation enemyAnimation;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        groundMovement = GetComponent<GroundEnemyMovement>();
        flyingMovement = GetComponent<FlyingEnemyMovement>();
        rangedCombat = GetComponent<EnemyRangeCombat>();
        meleeCombat = GetComponent<EnemyMeleeCombat>();
        targeting = GetComponent<EnemyTargeting>();
        formation = GetComponent<EnemyFormation>();
        stats = GetComponent<EnemyStats>();
        enemyAnimation = GetComponentInChildren<EnemyAnimation>();
        movement = GetComponent<EnemyMovementBase>();
    }
}
