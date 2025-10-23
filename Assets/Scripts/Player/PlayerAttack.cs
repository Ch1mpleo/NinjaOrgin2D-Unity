using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAttack : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private Weapon initialWeapon;
    [SerializeField] private Transform[] attackPositions;

    [Header("Melee Config")]
    [SerializeField] private ParticleSystem slashFX;
    [SerializeField] private float minDistanceMeleeAttack;

    [Header("Auto Targeting")]
    [SerializeField] private bool autoTargetEnabled = true;
    [SerializeField] private float autoTargetRange = 5f;

    public Weapon CurrentWeapon { get; set; }

    private PlayerActions actions;
    private PlayerAnimations playerAnimations;
    private PlayerMovement playerMovement;
    private PlayerMana playerMana;
    private EnemyBrain enemyTarget;
    private Coroutine attackCoroutine;

    private Transform currentAttackPosition;
    private float currentAttackRotation;

    private void Awake()
    {
        actions = new PlayerActions();
        playerMana = GetComponent<PlayerMana>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimations = GetComponent<PlayerAnimations>();
    }

    private void Start()
    {
        WeaponManager.Instance.EquipWeapon(initialWeapon);
        actions.Attack.ClickAttack.performed += ctx => Attack();
    }

    private void Update()
    {
        // auto-select nearest enemy when enabled
        if (autoTargetEnabled)
        {
            AutoSelectNearestEnemy();
        }

        GetFirePosition();
    }

    private void Attack()
    {
        // if no target, try to auto-select one before attacking
        if (enemyTarget == null && autoTargetEnabled)
        {
            AutoSelectNearestEnemy();
        }

        if (enemyTarget == null) return;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }

        attackCoroutine = StartCoroutine(IEAttack());
    }

    private IEnumerator IEAttack()
    {
        if (currentAttackPosition == null) yield break;
        if (CurrentWeapon.WeaponType == WeaponType.Magic)
        {
            if (playerMana.CurrentMana < CurrentWeapon.RequiredMana) yield break;
            MagicAttack();
        }
        else
        {
            MeleeAttack();
        }

        playerAnimations.SetAttackAnimation(true);
        yield return new WaitForSeconds(0.5f);
        playerAnimations.SetAttackAnimation(false);
    }

    private void MagicAttack()
    {
        Quaternion rotation =
            Quaternion.Euler(new Vector3(0f, 0f, currentAttackRotation));
        Projectile projectile = Instantiate(CurrentWeapon.ProjectilePrefab,
            currentAttackPosition.position, rotation);
        projectile.Direction = Vector3.up;
        projectile.Damage = GetAttackDamage();
        playerMana.UseMana(CurrentWeapon.RequiredMana);
    }

    private void MeleeAttack()
    {
        slashFX.transform.position = currentAttackPosition.position;
        slashFX.Play();
        float currentDistanceToEnemy =
            Vector3.Distance(enemyTarget.transform.position, transform.position);
        if (currentDistanceToEnemy <= minDistanceMeleeAttack)
        {
            enemyTarget.GetComponent<IDamageable>().TakeDamage(GetAttackDamage());
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        CurrentWeapon = newWeapon;
        stats.TotalDamage = stats.BaseDamage + CurrentWeapon.Damage;
    }

    private float GetAttackDamage()
    {
        float damage = stats.BaseDamage;
        damage += CurrentWeapon.Damage;
        float randomPerc = Random.Range(0f, 100);
        if (randomPerc <= stats.CriticalChance)
        {
            damage += damage * (stats.CriticalDamage / 100f);
        }

        return damage;
    }

    private void GetFirePosition()
    {
        Vector2 moveDirection = playerMovement.MoveDirection;
        switch (moveDirection.x)
        {
            case > 0f:
                currentAttackPosition = attackPositions[1];
                currentAttackRotation = -90f;
                break;
            case < 0f:
                currentAttackPosition = attackPositions[3];
                currentAttackRotation = -270f;
                break;
        }

        switch (moveDirection.y)
        {
            case > 0f:
                currentAttackPosition = attackPositions[0];
                currentAttackRotation = 0f;
                break;
            case < 0f:
                currentAttackPosition = attackPositions[2];
                currentAttackRotation = -180f;
                break;
        }
    }

    private void AutoSelectNearestEnemy()
    {
        // keep current target if still valid and within range
        if (enemyTarget != null)
        {
            EnemyHealth h = enemyTarget.GetComponent<EnemyHealth>();
            if (h != null && h.CurrentHealth > 0f &&
                Vector3.Distance(transform.position, enemyTarget.transform.position) <= autoTargetRange)
            {
                return;
            }
        }

        // FindObjectsByType is available in newer Unity; fall back to FindObjectsOfType if not.
#if UNITY_2023_1_OR_NEWER
        EnemyBrain[] enemies = UnityEngine.Object.FindObjectsByType<EnemyBrain>(UnityEngine.FindObjectsSortMode.None);
#else
        EnemyBrain[] enemies = UnityEngine.Object.FindObjectsOfType<EnemyBrain>();
#endif
        float bestDist = float.MaxValue;
        EnemyBrain best = null;
        foreach (var e in enemies)
        {
            if (e == null) continue;
            EnemyHealth eh = e.GetComponent<EnemyHealth>();
            if (eh == null || eh.CurrentHealth <= 0f) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d <= autoTargetRange && d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        enemyTarget = best;

        // notify selection manager so selector sprite and other listeners update
        if (enemyTarget != null)
        {
            SelectionManager.SelectEnemyExternally(enemyTarget);
        }
        else
        {
            SelectionManager.ClearSelectionExternally();
        }
    }

    private void EnemySelectedCallback(EnemyBrain enemySelected)
    {
        enemyTarget = enemySelected;
    }

    private void NoEnemySelectionCallback()
    {
        enemyTarget = null;
    }

    private void OnEnable()
    {
        actions.Enable();
        SelectionManager.OnEnemySelectedEvent += EnemySelectedCallback;
        SelectionManager.OnNoSelectionEvent += NoEnemySelectionCallback;
        EnemyHealth.OnEnemyDeadEvent += NoEnemySelectionCallback;
    }

    private void OnDisable()
    {
        actions.Disable();
        SelectionManager.OnEnemySelectedEvent -= EnemySelectedCallback;
        SelectionManager.OnNoSelectionEvent -= NoEnemySelectionCallback;
        EnemyHealth.OnEnemyDeadEvent -= NoEnemySelectionCallback;
    }
}