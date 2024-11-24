using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    private MCS_Attack attackHandler;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackHandler == null)
        {
            var characterController = animator.GetComponentInParent<RPGCharacterController>();
            if (characterController != null)
            {
                attackHandler = characterController.GetHandler(HandlerTypes.Attack) as MCS_Attack;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackHandler != null)
        {
            // 如果退出状态时仍处于 Recovery 阶段，说明动画被打断
            if (attackHandler.CurrentAttackPhase == MCS_Attack.AttackPhase.Recovery)
            {
                attackHandler.OnAttackEnd();
            }
        }
    }
}