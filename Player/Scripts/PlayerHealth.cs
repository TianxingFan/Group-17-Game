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

        // ����ж������������ܻ�����  
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

        // ����ж�������������������  
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // ������Ҷ����ӳ�1����ȷ�����������ܲ���  
        Destroy(gameObject, 1f);
    }
}