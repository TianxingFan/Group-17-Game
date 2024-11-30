using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 如果有动画器，触发受击动画  
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("You Died");

        // 如果有动画器，触发死亡动画  
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 销毁玩家对象，延迟1秒以确保死亡动画能播放  
        Destroy(gameObject, 1f);
    }
}