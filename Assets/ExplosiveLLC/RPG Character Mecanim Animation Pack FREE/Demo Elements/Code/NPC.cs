using UnityEngine;
using RPGCharacterAnims.Lookups;

namespace RPGCharacterAnims
{
    [RequireComponent(typeof(RPGCharacterController))]
	[RequireComponent(typeof(RPGCharacterNavigationController))]
	public class NPC : MonoBehaviour
    {
        private RPGCharacterController rpgCharacterController;
        private RPGCharacterNavigationController rpgNavigationController;
		private MeleeCombatSystem meleeCombatSystem;
        private Vector3 targetPosition;
		public float followDistance = 3f;
		private float outOfRangeTimer = 0f;
		private const float attackDelay = 3f;

		void Awake()
		{
            rpgCharacterController = GetComponent<RPGCharacterController>();
            rpgNavigationController = GetComponent<RPGCharacterNavigationController>();
			meleeCombatSystem = GetComponent<MeleeCombatSystem>();
		}

		private void Update()
		{
			targetPosition = rpgCharacterController.target.transform.position;
            if (IsOutOfRange(transform.position, targetPosition))
            {
                outOfRangeTimer += Time.deltaTime;
                if (outOfRangeTimer >= attackDelay)
                {
                    MoveAndAttack();
                }
            }
            else
            {
                outOfRangeTimer = 0f;
                if (rpgCharacterController.CanEndAction(HandlerTypes.Navigation))
                {
                    rpgCharacterController.EndAction(HandlerTypes.Navigation);
                }
            }
			// if (IsOutOfRange(transform.position, targetPosition))
			// { rpgCharacterController.StartAction(HandlerTypes.Navigation, RandomOffset(targetPosition)); }
		}

		private void MoveAndAttack()
        {
            Vector3 attackPosition = targetPosition + (transform.position - targetPosition).normalized * 4f;
            rpgCharacterController.StartAction(HandlerTypes.Navigation, attackPosition);
            
            // 等待移动完成后攻击
            StartCoroutine(AttackAfterMove());
        }

        private System.Collections.IEnumerator AttackAfterMove()
        {
            yield return new WaitUntil(() => !rpgNavigationController.isNavigating);
            
            // 面向目标
            transform.LookAt(targetPosition);

            if (!rpgCharacterController.hasTwoHandedWeapon)
            {
	            meleeCombatSystem.EquipWeapon(Weapon.TwoHandSword);
	            rpgCharacterController.leftWeapon = Weapon.TwoHandSword;
	            rpgCharacterController.rightWeapon = Weapon.TwoHandSword;
            }
            // 发起攻击
            var attackHandler = rpgCharacterController.GetHandler(HandlerTypes.Attack) as AttackHandler;
            meleeCombatSystem.PerformAttack(1, Side.Right);
            
            outOfRangeTimer = 0f;
        }

		private Vector3 RandomOffset(Vector3 position)
		{ return new Vector3(position.x - Random.Range(1, 2), position.y, position.z - Random.Range(1, 2)); }

		private bool IsOutOfRange(Vector3 npc, Vector3 player)
		{
			if (Vector3.Distance(npc, player) > followDistance) { return true; }
			else { return false; }
		}
	}
}