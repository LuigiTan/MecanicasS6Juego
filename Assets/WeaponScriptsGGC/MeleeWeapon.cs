using UnityEngine;
using UnityEngine.UI;

public class MeleeWeapon : BaseWeapon
{
    [Header("Melee Settings")]
    public float attackRange = 2f;
    public float attackRadius = 1.2f;
    public Transform attackOrigin;
    public LayerMask enemyMask;

    public Slider chargeBar;
    public float fillSpeed = 0.5f;
    public bool isCharging;

    [Header("Preparaci�n")]
    public Transform weaponModel;
    public Vector3 readyOffset = new Vector3(0f, -0.1f, -0.4f);
    public Vector3 attackOffset = new Vector3(0f, -10f, 0f);
    public float animationSpeed = 5f;

    private Vector3 defaultPosition;
    private bool isPreparing = false;
    private bool isReadyToAttack = false;

    void Start()
    {
        if (weaponModel != null)
            defaultPosition = weaponModel.localPosition;

        chargeBar.value = 0f;
        chargeBar.gameObject.SetActive(false);
        isCharging = false;
    }

    void Update()
    {
        AnimatePreparation();

        if (isCharging && chargeBar.value < chargeBar.maxValue)
            chargeBar.value += fillSpeed * Time.deltaTime;
    }

    public void BeginPreparation()
    {
        isPreparing = true;
        chargeBar.gameObject.SetActive(true);
        isCharging = true;
    }

    public void CancelPreparation()
    {
        isPreparing = false;
        isReadyToAttack = false;
        chargeBar.value = 0;
        chargeBar.gameObject.SetActive(false);
        isCharging = false;
    }

    public void ConfirmReady()
    {
        if (isPreparing && chargeBar.value == chargeBar.maxValue)
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

        chargeBar.value = 0;
        Debug.Log("Ataque melee ejecutado. Enemigos golpeados: " + hits.Length);
        Vector3 targetPos = defaultPosition + attackOffset;
        weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, targetPos, Time.deltaTime * animationSpeed);
    }

    private void AnimatePreparation()
    {
        if (weaponModel == null) return;

        if (isReadyToAttack) return;

        Vector3 targetPos = isPreparing ? defaultPosition + readyOffset : defaultPosition;
        weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, targetPos, Time.deltaTime * animationSpeed);
    }

    // Visualizaci�n en escena
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position + attackOrigin.forward * attackRange, attackRadius);
    }
}
