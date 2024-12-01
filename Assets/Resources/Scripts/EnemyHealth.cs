using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // 最大生命值，可在 Inspector 中为每个敌人单独设置  
    protected float currentHealth;
    protected Animator animator;
    protected bool isDead = false;

    public bool IsDead => isDead;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void TakeHit(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // 触发受击动画  
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // 检查是否死亡  
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        // 触发死亡动画  
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 禁用碰撞体  
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 禁用移动和 AI 脚本  
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // 停止物理模拟  
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 启动协程等待死亡动画播放完毕  
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 获取死亡动画的时长  
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
}