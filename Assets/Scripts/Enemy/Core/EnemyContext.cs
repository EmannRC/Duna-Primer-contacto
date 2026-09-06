using UnityEngine;

public class EnemyContext : MonoBehaviour
{
    [Header("Core")]
    public EnemyStats stats;
    public EnemyHealth health;
    public GroundEnemyMovement groundMovement;
    public FlyingEnemyMovement flyingMovement;
    
    public EnemyRangeCombat rangedCombat;
    public EnemyMeleeCombat meleeCombat;
    public EnemyTargeting targeting;
    public EnemyFormation formation;

    public Animator animator;
    public EnemyAnimationSync animationSync;

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
        animationSync = GetComponent<EnemyAnimationSync>();
    }
}
