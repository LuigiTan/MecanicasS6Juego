
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEnemy
{
    public float health = 50f;
    public float detectionRadius = 4f;
    public float attackRange = 1.2f;
    public float damage = 5f;
    public float attackDelay = 1f;

    [Header("Movement")]
    public float baseSpeed = 3.5f;
    public float speedModifier = 1.0f; // Multiplier

    private Transform player;
    private TrapBase currentTargetTrap;
    private Transform goal;
    private NavMeshAgent agent;
    private float lastAttack;
    private bool isStunned = false;
    public bool IsStunned() => isStunned;

    [Header("Economy")]
    public int moneyReward = 25;
    private bool isDead = false;

    private Base baseScript;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        goal = GameObject.Find("Goal").transform;
        baseScript = goal?.GetComponent<Base>();
        agent = GetComponent<NavMeshAgent>();

        float healthMultiplier = EnemyScaler.GetHealthMultiplier();
        float damageMultiplier = EnemyScaler.GetDamageMultiplier();
        float speedMultiplier = EnemyScaler.GetSpeedMultiplier();

        health *= healthMultiplier;
        damage *= damageMultiplier;
        agent.speed = baseSpeed * speedModifier * speedMultiplier; // scaled speed

        Debug.Log($"[Enemy Spawned] \nHorde: {EnemyScaler.HordeCount}\n HP: {health}\n DMG: {damage}\n Speed: {agent.speed}");
    }


    void Update()
    {
        if (isStunned)
        {
            // Don’t let Update override isStopped state
            return;
        }

        float distToPlayer = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        if (distToPlayer < detectionRadius)
        {
            agent.SetDestination(player.position);
            TryAttackPlayer();
        }
        else if (currentTargetTrap != null)
        {
            agent.SetDestination(currentTargetTrap.transform.position);
            TryAttackTrap();
        }
        else
        {
            TrapBase[] traps = FindObjectsOfType<TrapBase>();
            foreach (var trap in traps)
            {
                float d = Vector3.Distance(transform.position, trap.transform.position);
                if (d < detectionRadius)
                {
                    currentTargetTrap = trap;
                    break;
                }
            }

            agent.SetDestination(goal.position);
        }

        if (goal != null && Vector3.Distance(transform.position, goal.position) < attackRange)
        {
            TryAttackGoal();
        }
    }

    private void TryAttackGoal()
    {
        if (baseScript == null) return;
        if (Time.time - lastAttack > attackDelay)
        {
            baseScript.TakeDamage(damage);
            lastAttack = Time.time;
        }

    }

    private void TryAttackPlayer()
    {
        if (player == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        if (d < attackRange && Time.time - lastAttack > attackDelay)
        {
            player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            lastAttack = Time.time;
        }
    }

    private void TryAttackTrap()
    {
        if (currentTargetTrap == null) return;

        float d = Vector3.Distance(transform.position, currentTargetTrap.transform.position);
        if (d < attackRange && Time.time - lastAttack > attackDelay)
        {
            currentTargetTrap.TakeDamage(damage);
            lastAttack = Time.time;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        PlayerStats.Instance?.AddMoney(moneyReward);
        Destroy(gameObject);
    }


    private Coroutine stunCoroutine;

    private float stunCooldownTime = 0f;

    public void ApplyStun(float seconds)
    {
        if (Time.time < stunCooldownTime)
            return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCooldownTime = Time.time + seconds;
        stunCoroutine = StartCoroutine(StunRoutine(seconds));
    }


    private System.Collections.IEnumerator StunRoutine(float time)
    {
        isStunned = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // forcibly kill movement

        yield return new WaitForSeconds(time);

        isStunned = false;
        agent.isStopped = false;
    }




    public bool IsAlive() => health > 0;
    public Transform GetTransform() => transform;
}
