using UnityEngine;
using System.Collections;

public class SlimeController : MonoBehaviour
{
    // 基础属性  
    public float moveSpeed = 2f;             // 巡逻移动速度  
    public float chaseSpeed = 3f;            // 追击移动速度  
    public float fleeSpeed = 4f;             // 逃跑移动速度  
    public float detectionRange = 5f;        // 检测玩家的范围  
    public float attackRange = 1f;           // 攻击范围  
    public float attackCooldown = 2f;        // 攻击冷却时间  
    public int damage = 10;                  // 对玩家造成的伤害  
    public float maxHealth = 100f;           // 最大生命值  
    public float fleeHealthThreshold = 20f;  // 开始逃跑的生命值阈值  

    // 引用  
    private Transform player;                // 玩家  
    public LayerMask playerLayer;            // 玩家层  
    public Transform[] patrolPoints;         // 巡逻点  
    private int currentPatrolIndex = 0;      // 当前巡逻点索引  

    // 动画和物理  
    private Animator animator;               // Animator 组件  
    private Rigidbody2D rb;                  // Rigidbody2D 组件  
    private float attackTimer = 0f;          // 攻击计时器  
    private bool isDead = false;             // 是否已死亡  

    // 生命值  
    private float currentHealth;

    // 定义 Slime 的 AI 状态  
    private enum SlimeState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Hurt,
        Death
    }

    private SlimeState currentState;         // 当前状态  

    void Awake()
    {
        currentHealth = maxHealth;           // 初始化生命值  
        animator = GetComponent<Animator>(); // 获取 Animator  
        rb = GetComponent<Rigidbody2D>();    // 获取 Rigidbody2D  

        // 检查并初始化巡逻点  
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("巡逻点数组未设置或为空，请在Inspector中设置巡逻点。");
            currentState = SlimeState.Idle;
            return;
        }

        // 找到最近的巡逻点  
        float closestDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        currentState = SlimeState.Patrol;    // 初始状态为巡逻  
    }

    void Update()
    {
        if (isDead)
            return;

        attackTimer += Time.deltaTime;       // 更新攻击计时器  
        UpdateState();                       // 更新状态  
        HandleStateActions();                // 执行状态行为  
    }

    // 更新当前状态  
    void UpdateState()
    {
        // 检测玩家  
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (playerCollider != null)
        {
            player = playerCollider.transform;

            if (currentHealth <= fleeHealthThreshold)     // 生命值低于阈值，逃跑  
            {
                currentState = SlimeState.Flee;
            }
            else
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)      // 在攻击范围内，攻击  
                {
                    currentState = SlimeState.Attack;
                }
                else                                      // 否则，追击  
                {
                    currentState = SlimeState.Chase;
                }
            }
        }
        else
        {
            player = null;

            if (currentState != SlimeState.Patrol && currentState != SlimeState.Idle)
            {
                currentState = SlimeState.Patrol;         // 玩家离开，返回巡逻  
            }
        }
    }

    // 根据当前状态执行相应行为  
    void HandleStateActions()
    {
        switch (currentState)
        {
            case SlimeState.Idle:
                Idle();
                break;

            case SlimeState.Patrol:
                Patrol();
                break;

            case SlimeState.Chase:
                ChasePlayer();
                break;

            case SlimeState.Attack:
                ChasePlayer(); // 继续靠近玩家  
                break;

            case SlimeState.Flee:
                FleeFromPlayer();
                break;

            case SlimeState.Hurt:
                // 如果需要，添加受伤后的逻辑  
                break;

            case SlimeState.Death:
                // 死亡已在 Die() 方法中处理  
                break;
        }
    }

    // Idle 状态行为  
    void Idle()
    {
        rb.velocity = Vector2.zero;
        // 保持 Idle 动画  
    }

    // Patrol 状态行为  
    void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            currentState = SlimeState.Idle;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = ((Vector2)targetPoint.position - rb.position);

        if (direction.sqrMagnitude < 0.1f) // 到达目标点  
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            rb.velocity = Vector2.zero; // 停止移动  
            return;
        }

        direction = direction.normalized;
        rb.velocity = direction * moveSpeed;
        FaceDirection(direction);
    }

    // Chase 状态行为  
    void ChasePlayer()
    {
        if (player == null)
        {
            currentState = SlimeState.Patrol;
            return;
        }

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.velocity = direction * chaseSpeed;
        FaceDirection(direction);
    }

    // Flee 状态行为  
    void FleeFromPlayer()
    {
        if (player == null)
        {
            currentState = SlimeState.Patrol;
            return;
        }

        Vector2 direction = ((Vector2)transform.position - (Vector2)player.position).normalized;
        rb.velocity = direction * fleeSpeed;
        FaceDirection(direction);
    }

    // 受伤处理  
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
        {
            animator.SetTrigger("Hit"); // 触发受击动画  
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = SlimeState.Hurt;
            // 如果需要，添加受伤后的逻辑  
        }
    }

    // 死亡处理  
    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die"); // 触发死亡动画  
        }

        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // 防止死亡后继续受到物理影响  

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 禁用其他脚本  
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // 启动协程等待死亡动画播放完毕  
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 等待进入死亡动画状态  
        while (!stateInfo.IsName("Die"))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // 等待死亡动画播放完毕  
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);

        // 销毁游戏对象  
        Destroy(gameObject);
    }

    // 碰撞检测，用于攻击玩家  
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && attackTimer >= attackCooldown)
        {
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                attackTimer = 0f;
            }
            else
            {
                Debug.LogWarning("碰撞到的玩家没有 PlayerHealth 组件。");
            }
        }
    }

    // 可视化检测范围  
    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围  
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制攻击范围  
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // 面向移动方向的方法  
    void FaceDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // 面向右  
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // 面向左  
        }
    }
}