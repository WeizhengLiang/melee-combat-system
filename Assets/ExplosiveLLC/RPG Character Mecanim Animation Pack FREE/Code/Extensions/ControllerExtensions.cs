using UnityEngine;

namespace RPGCharacterAnims.Extensions
{
    public static class ControllerExtensions
    {
        public static void DebugController(this RPGCharacterController controller)
        {
            Debug.Log("CONTROLLER SETTINGS---------------------------");
			Debug.Log($"AnimationSpeed:{controller.animationSpeed}   headLook:{controller.headLook}   " +
				$"isHeadlook:{controller.isHeadlook}    ladder:{controller.ladder}    cliff:{controller.cliff}  " +
				$"canAction:{controller.canAction}   canMove:{controller.canMove}");
			Debug.Log($"   acquiringGround:{controller.acquiringGround}    " +
				$"maintainingGround:{controller.maintainingGround}    isAttacking:{controller.isAttacking}    " +
				$"isBlocking:{controller.isBlocking}");
			Debug.Log($"isCrouching: isDead:{controller.isDead}    " +
				$"isFacing:isFalling:{controller.isFalling}    " +
				$"isIdle:{controller.isIdle}    ");
			Debug.Log($"   isMoving:{controller.isMoving}    " +
				$"isNearCliff:{controller.isNearCliff}    isNearLadder:{controller.isNearLadder}  " +
				$"isRolling:{controller.isRolling}    isKnockback:{controller.isKnockback}");
			Debug.Log($"isKnockdown:{controller.isKnockdown}    isSitting:{controller.isSitting}    isSpecial:{controller.isSpecial}    " +
			          $"isTalking:{controller.isTalking}    moveInput:{controller.moveInput}");
			Debug.Log($"aimInput:{controller.aimInput}    jumpInput:{controller.jumpInput}    cameraRelativeInput:{controller.cameraRelativeInput}    " +
				$"_bowPull:{controller.bowPull}    rightWeapon:{controller.rightWeapon}    leftWeapon:{controller.leftWeapon}");
			Debug.Log($"hasRightWeapon:{controller.hasRightWeapon}    hasLeftWeapon:{controller.hasLeftWeapon}    hasDualWeapons:{controller.hasDualWeapons}    " +
				$"hasTwoHandedWeapon:{controller.hasTwoHandedWeapon}    hasShield:{controller.hasShield}");
        }
    }
}