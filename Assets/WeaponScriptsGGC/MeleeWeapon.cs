using UnityEngine;

public class MeleeWeapon : BaseWeapon
{
    [Header("Melee Settings")]
    public float attackRange = 2f;
    public float attackRadius = 1.2f;
    public Transform attackOrigin;
    public LayerMask enemyMask;

    [Header("Preparación")]
    public Transform weaponModel;
    public Vector3 readyOffset = new Vector3(0f, -0.1f, -0.4f);
    public float animationSpeed = 5f;

    private Vector3 defaultPosition;
    private bool isPreparing = false;
    private bool isReadyToAttack = false;

    void Start()
    {
        if (weaponModel != null)
            defaultPosition = weaponModel.localPosition;
    }

    void Update()
    {
        AnimatePreparation();
    }

    public void BeginPreparation()
    {
        isPreparing = true;
    }

    public void CancelPreparation()
    {
        isPreparing = false;
        isReadyToAttack = false;
    }

    public void ConfirmReady()
    {
        if (isPreparing)
            isReadyToAttack = true;
    }

    protected override void Attack()
    {
        if (!isReadyToAttack) return;

        isReadyToAttack = false;

        Collider[] hits = Physics.OverlapSphere(attackOrigin.position + attackOrigin.forward * attackRange, attackRadius, enemyMask);

        foreach (Collider col in hits)
        {
            IEnemy enemy = col.GetComponent<IEnemy>();
            if (enemy != null && enemy.IsAlive())
            {
                enemy.TakeDamage(damage);
            }
        }

        Debug.Log("Ataque melee ejecutado. Enemigos golpeados: " + hits.Length);
    }

    private void AnimatePreparation()
    {
        if (weaponModel == null) return;

        Vector3 targetPos = isPreparing ? defaultPosition + readyOffset : defaultPosition;
        weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, targetPos, Time.deltaTime * animationSpeed);
    }

    // Visualización en escena
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position + attackOrigin.forward * attackRange, attackRadius);
    }
}
