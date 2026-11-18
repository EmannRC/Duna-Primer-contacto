using UnityEngine;

public class AttackStateCleanup : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        var combat = animator.GetComponentInParent<PlayerCombat>();
        combat?.EndAttack();
    }
}
