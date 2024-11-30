using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float attackAngle = 60f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private bool isDead = false;
    private bool canAttack = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            rb.AddForce(moveInput * moveSpeed);
        }
    }

    private void OnMove(InputValue value)
    {
        if (isDead) return;

        moveInput = value.Get<Vector2>();
        animator.SetBool("isMoving", moveInput != Vector2.zero);

        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;

            if (attackHitbox != null)
            {
                Vector3 hitboxPos = attackHitbox.localPosition;
                hitboxPos.x = Mathf.Abs(hitboxPos.x) * (moveInput.x < 0 ? -1 : 1);
                attackHitbox.localPosition = hitboxPos;
            }
        }
    }

    void OnFire()
    {
        if (isDead || !canAttack) return;

        animator.SetTrigger("Attack");
        canAttack = false;

        float attackDirection = spriteRenderer.flipX ? -1 : 1;

        // 获取所有在攻击范围内的碰撞体  
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackHitbox.position, attackRange);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            // 检查是否具有 "Enemy" 标签  
            if (hitCollider.CompareTag("Enemy"))
            {
                Vector2 directionToEnemy = (hitCollider.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(new Vector2(attackDirection, 0), directionToEnemy);

                if (angle <= attackAngle)
                {
                    // 使用 SendMessage 调用敌人的 TakeDamage 方法  
                    hitCollider.gameObject.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}