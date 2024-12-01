using UnityEngine;
using System.Collections;

public class SlimeController : MonoBehaviour
{
    // ��������  
    public float moveSpeed = 2f;             // Ѳ���ƶ��ٶ�  
    public float chaseSpeed = 3f;            // ׷���ƶ��ٶ�  
    public float fleeSpeed = 4f;             // �����ƶ��ٶ�  
    public float detectionRange = 5f;        // �����ҵķ�Χ  
    public float attackRange = 1f;           // ������Χ  
    public float attackCooldown = 2f;        // ������ȴʱ��  
    public int damage = 10;                  // �������ɵ��˺�  
    public float maxHealth = 100f;           // �������ֵ  
    public float fleeHealthThreshold = 20f;  // ��ʼ���ܵ�����ֵ��ֵ  

    // ����  
    private Transform player;                // ���  
    public LayerMask playerLayer;            // ��Ҳ�  
    public Transform[] patrolPoints;         // Ѳ�ߵ�  
    private int currentPatrolIndex = 0;      // ��ǰѲ�ߵ�����  

    // ����������  
    private Animator animator;               // Animator ���  
    private Rigidbody2D rb;                  // Rigidbody2D ���  
    private float attackTimer = 0f;          // ������ʱ��  
    private bool isDead = false;             // �Ƿ�������  

    // ����ֵ  
    private float currentHealth;

    // ���� Slime �� AI ״̬  
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

    private SlimeState currentState;         // ��ǰ״̬  

    void Awake()
    {
        currentHealth = maxHealth;           // ��ʼ������ֵ  
        animator = GetComponent<Animator>(); // ��ȡ Animator  
        rb = GetComponent<Rigidbody2D>();    // ��ȡ Rigidbody2D  

        // ��鲢��ʼ��Ѳ�ߵ�  
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("Ѳ�ߵ�����δ���û�Ϊ�գ�����Inspector������Ѳ�ߵ㡣");
            currentState = SlimeState.Idle;
            return;
        }

        // �ҵ������Ѳ�ߵ�  
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
        currentState = SlimeState.Patrol;    // ��ʼ״̬ΪѲ��  
    }

    void Update()
    {
        if (isDead)
            return;

        attackTimer += Time.deltaTime;       // ���¹�����ʱ��  
        UpdateState();                       // ����״̬  
        HandleStateActions();                // ִ��״̬��Ϊ  
    }

    // ���µ�ǰ״̬  
    void UpdateState()
    {
        // ������  
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (playerCollider != null)
        {
            player = playerCollider.transform;

            if (currentHealth <= fleeHealthThreshold)     // ����ֵ������ֵ������  
            {
                currentState = SlimeState.Flee;
            }
            else
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)      // �ڹ�����Χ�ڣ�����  
                {
                    currentState = SlimeState.Attack;
                }
                else                                      // ����׷��  
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
                currentState = SlimeState.Patrol;         // ����뿪������Ѳ��  
            }
        }
    }

    // ���ݵ�ǰ״ִ̬����Ӧ��Ϊ  
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
                ChasePlayer(); // �����������  
                break;

            case SlimeState.Flee:
                FleeFromPlayer();
                break;

            case SlimeState.Hurt:
                // �����Ҫ��������˺���߼�  
                break;

            case SlimeState.Death:
                // �������� Die() �����д���  
                break;
        }
    }

    // Idle ״̬��Ϊ  
    void Idle()
    {
        rb.velocity = Vector2.zero;
        // ���� Idle ����  
    }

    // Patrol ״̬��Ϊ  
    void Patrol()
    {
        if (patrolPoints.Length == 0)
        {
            currentState = SlimeState.Idle;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = ((Vector2)targetPoint.position - rb.position);

        if (direction.sqrMagnitude < 0.1f) // ����Ŀ���  
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            rb.velocity = Vector2.zero; // ֹͣ�ƶ�  
            return;
        }

        direction = direction.normalized;
        rb.velocity = direction * moveSpeed;
        FaceDirection(direction);
    }

    // Chase ״̬��Ϊ  
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

    // Flee ״̬��Ϊ  
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

    // ���˴���  
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (animator != null)
        {
            animator.SetTrigger("Hit"); // �����ܻ�����  
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = SlimeState.Hurt;
            // �����Ҫ��������˺���߼�  
        }
    }

    // ��������  
    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die"); // ������������  
        }

        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // ��ֹ����������ܵ�����Ӱ��  

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // ���������ű�  
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // ����Э�̵ȴ����������������  
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // �ȴ�������������״̬  
        while (!stateInfo.IsName("Die"))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // �ȴ����������������  
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);

        // ������Ϸ����  
        Destroy(gameObject);
    }

    // ��ײ��⣬���ڹ������  
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
                Debug.LogWarning("��ײ�������û�� PlayerHealth �����");
            }
        }
    }

    // ���ӻ���ⷶΧ  
    private void OnDrawGizmosSelected()
    {
        // ���Ƽ�ⷶΧ  
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // ���ƹ�����Χ  
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // �����ƶ�����ķ���  
    void FaceDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // ������  
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // ������  
        }
    }
}