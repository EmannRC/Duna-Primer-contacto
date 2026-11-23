using UnityEngine;

public class AttackAnimationSpeed : StateMachineBehaviour
{
    [SerializeField] private float referenceAttackSpeed = 1f;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        PlayerContext ctx =
            animator.GetComponentInParent<PlayerContext>();

        if (ctx == null || ctx.stats == null)
            return;

        float attackSpeed =
            ctx.stats.GetStat(StatType.AttackSpeed);

        if (attackSpeed <= 0f)
            attackSpeed = 0.1f;

        float animationSpeed =
            attackSpeed / referenceAttackSpeed;

        animator.SetFloat(
            "AttackAnimationSpeed",
            animationSpeed
        );
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetFloat(
            "AttackAnimationSpeed",
            1f
        );
    }
}
