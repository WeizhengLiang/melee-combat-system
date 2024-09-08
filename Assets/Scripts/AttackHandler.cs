using System.Collections;
using System.Collections.Generic;
using RPGCharacterAnims;
using RPGCharacterAnims.Lookups;
using UnityEngine;

public class AttackHandler : MonoBehaviour, IAttacker
{
    private RPGCharacterController characterController;
    public float attackDuration = 0.5f;

    public WeaponAttackDataSO weaponAttackDataSO;

    private Dictionary<Weapon, GameObject> weaponPrefabs = new Dictionary<Weapon, GameObject>();

    [Header("Debug")]
    public bool debugMode = true;

    private void Awake()
    {
        characterController = GetComponent<RPGCharacterController>();
        CacheWeaponPrefabs();
    }

    private void CacheWeaponPrefabs()
    {
        if (debugMode) Debug.Log("AttackHandler: Starting to cache weapon prefabs");

        // 确保 WeaponManager 实例存在
        if (WeaponManager.Instance == null)
        {
            Debug.LogError("AttackHandler: WeaponManager instance not found. Make sure it's set up in the scene.");
            return;
        }

        // 从 WeaponManager 获取所有武器预制体
        weaponPrefabs = WeaponManager.Instance.GetAllWeaponPrefabs();

        if (debugMode)
        {
            if (weaponPrefabs.Count == 0)
            {
                Debug.LogWarning("AttackHandler: No weapon prefabs cached");
            }
            else
            {
                Debug.Log($"AttackHandler: Successfully cached {weaponPrefabs.Count} weapon prefabs");
                foreach (var weapon in weaponPrefabs.Keys)
                {
                    Debug.Log($"AttackHandler: Cached weapon prefab for {weapon}");
                }
            }
        }

        // 验证武器数据
        ValidateWeaponData();
    }

    private void ValidateWeaponData()
    {
        if (weaponAttackDataSO == null)
        {
            Debug.LogError("AttackHandler: WeaponAttackDataSO is not assigned. Please assign it in the inspector.");
            return;
        }

        foreach (var weapon in weaponPrefabs.Keys)
        {
            if (weaponAttackDataSO.GetAttackPoints(weapon).Length == 0)
            {
                Debug.LogWarning($"AttackHandler: No attack points defined for {weapon} in WeaponAttackDataSO.");
            }
        }
    }

    public void PerformAttack(int attackNumber,Side attackSide)
    {
        if (debugMode) Debug.Log($"AttackHandler: Performing attack from {attackSide} side");
        characterController.Attack(attackNumber, attackSide, characterController.leftWeapon, characterController.rightWeapon, attackDuration);
        StartCoroutine(DetectHitDuringAnimation(attackSide));
    }

    private IEnumerator DetectHitDuringAnimation(Side attackSide)
    {
        if (debugMode) Debug.Log("AttackHandler: Starting hit detection coroutine");
        float elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            DetectHit(attackSide);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (debugMode) Debug.Log("AttackHandler: Finished hit detection coroutine");
    }

    private void DetectHit(Side attackSide)
    {
        Weapon currentWeapon = (attackSide == Side.Left) ? characterController.leftWeapon : characterController.rightWeapon;
        if (debugMode) Debug.Log($"AttackHandler: Detecting hit for {currentWeapon} on {attackSide} side");

        WeaponAttackDataSO.AttackPoint[] attackPoints = weaponAttackDataSO.GetAttackPoints(currentWeapon);
        float currentRadius = GetCurrentAttackRadius(currentWeapon);

        if (weaponPrefabs.TryGetValue(currentWeapon, out GameObject weaponPrefab))
        {
            foreach (var attackPoint in attackPoints)
            {
                Vector3 worldAttackPoint = weaponPrefab.transform.TransformPoint(attackPoint.localOffset);
                if (debugMode) Debug.Log($"AttackHandler: Checking hit at {worldAttackPoint} with radius {currentRadius}");

                Collider[] hitColliders = Physics.OverlapSphere(worldAttackPoint, currentRadius);
                foreach (var hitCollider in hitColliders)
                {
                    IDamageable damageable = hitCollider.GetComponent<IDamageable>();
                    if (damageable != null && hitCollider.gameObject != this.gameObject)
                    {
                        if (debugMode) Debug.Log($"AttackHandler: Hit detected on {hitCollider.gameObject.name}");
                        damageable.ReceiveHit(worldAttackPoint);
                    }
                }
            }
        }
        else
        {
            if (debugMode) Debug.LogWarning($"AttackHandler: Weapon prefab not found for {currentWeapon}");
        }
    }

    private float GetCurrentAttackRadius(Weapon weapon)
    {
        float radius = weaponAttackDataSO.GetAttackRadius(weapon);
        if (debugMode) Debug.Log($"AttackHandler: Attack radius for {weapon} is {radius}");
        return radius;
    }

    public Vector3 GetAttackPosition(Side attackSide)
    {
        Weapon currentWeapon = (attackSide == Side.Left) ? characterController.leftWeapon : characterController.rightWeapon;
        if (weaponPrefabs.TryGetValue(currentWeapon, out GameObject weaponPrefab))
        {
            WeaponAttackDataSO.AttackPoint[] points = weaponAttackDataSO.GetAttackPoints(currentWeapon);
            if (points.Length > 0)
            {
                Vector3 attackPosition = weaponPrefab.transform.TransformPoint(points[0].localOffset);
                if (debugMode) Debug.Log($"AttackHandler: Attack position for {currentWeapon} on {attackSide} side is {attackPosition}");
                return attackPosition;
            }
        }
        if (debugMode) Debug.LogWarning($"AttackHandler: Could not find attack position for {currentWeapon} on {attackSide} side, returning character position");
        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponAttackDataSO == null) return;

        Gizmos.color = Color.red;
        DrawWeaponGizmos(characterController.rightWeapon);

        Gizmos.color = Color.blue;
        DrawWeaponGizmos(characterController.leftWeapon);
    }

    private void DrawWeaponGizmos(Weapon weapon)
    {
        if (weaponPrefabs.TryGetValue(weapon, out GameObject weaponPrefab))
        {
            float radius = Application.isPlaying ? GetCurrentAttackRadius(weapon) : weaponAttackDataSO.GetAttackRadius(weapon);
            WeaponAttackDataSO.AttackPoint[] points = weaponAttackDataSO.GetAttackPoints(weapon);

            foreach (var point in points)
            {
                Vector3 worldPoint = weaponPrefab.transform.TransformPoint(point.localOffset);
                Gizmos.DrawWireSphere(worldPoint, radius);
                if (debugMode && Application.isPlaying) Debug.DrawLine(transform.position, worldPoint, Color.yellow, 0.1f);
            }
        }
        else if (debugMode && Application.isPlaying)
        {
            Debug.LogWarning($"AttackHandler: Cannot draw gizmos for {weapon}, prefab not found");
        }
    }
}