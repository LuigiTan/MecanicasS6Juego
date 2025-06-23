
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IEnemy
{
    public float health = 50f;
    public float detectionRadius = 4f;
    public float attackRange = 1.2f;
    public float damage = 5f;
    public float attackDelay = 1f;

    private Transform player;
    private TrapBase currentTargetTrap;
    private Transform goal;
    private NavMeshAgent agent;
    private float lastAttack;
    private bool isStunned = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        goal = GameObject.Find("Goal").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isStunned)
        {
            agent.isStopped = true;
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
        health -= amount;
        if (health <= 0)
        {
            PlayerStats.Instance.AddMoney(25);
            Destroy(gameObject);
        }
    }

    public void ApplyStun(float seconds)
    {
        StartCoroutine(StunRoutine(seconds));
    }

    public bool IsAlive() => health > 0;
    public Transform GetTransform() => transform;

    private System.Collections.IEnumerator StunRoutine(float time)
    {
        isStunned = true;
        yield return new WaitForSeconds(time);
        isStunned = false;
    }
}
