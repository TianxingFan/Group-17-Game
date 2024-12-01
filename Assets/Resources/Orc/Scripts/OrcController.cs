using UnityEngine;
using System.Collections;

public class OrcController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float waitTime = 2f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 5f;
    private Vector2 patrolTarget;
    private float waitCounter;

    [Header("Chase Settings")]
    [SerializeField] private float chaseRadius = 6f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 20;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    // 状态管理  
    private bool isChasing = false;
    private GameObject currentTarget;
    private Vector2 movement;
    private bool canAttack = true;
    private float attackTimer = 0f;

    // 组件引用  
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isChasing && currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (distanceToTarget <= attackRange)
            {
                if (canAttack)
                {
                    Attack();
                }
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }

        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
            }
        }

        UpdateMovement();
        UpdateAnimation();
    }

    private void SetNewPatrolTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
        patrolTarget = (Vector2)transform.position + randomDirection;
        waitCounter = waitTime;
    }

    private void Patrol()
    {
        if (waitCounter > 0)
        {
            waitCounter -= Time.deltaTime;
            movement = Vector2.zero;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, patrolTarget);
        if (distanceToTarget < 0.1f)
        {
            SetNewPatrolTarget();
            return;
        }

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        movement = direction * patrolSpeed;
        UpdateSpriteDirection(direction.x);
    }

    private void ChasePlayer()
    {
        if (currentTarget != null)
        {
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
            movement = direction * chaseSpeed;
            UpdateSpriteDirection(direction.x);
        }
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");
        canAttack = false;
        attackTimer = 0f;

        if (currentTarget != null)
        {
            PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void UpdateMovement()
    {
        if (!isDead)
        {
            rb.velocity = movement;
        }
    }

    private void UpdateAnimation()
    {
        if (isDead) return;

        bool isMoving = movement.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);
    }

    private void UpdateSpriteDirection(float horizontalDirection)
    {
        if (horizontalDirection != 0)
        {
            spriteRenderer.flipX = horizontalDirection < 0;
        }
    }

    public void OnPlayerDetected(GameObject player)
    {
        isChasing = true;
        currentTarget = player;
    }

    public void OnPlayerLost()
    {
        isChasing = false;
        currentTarget = null;
        SetNewPatrolTarget();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);  // 假设死亡动画时间  
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}