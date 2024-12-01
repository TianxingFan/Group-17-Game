using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f; // �������ֵ������ Inspector ��Ϊÿ�����˵�������  
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

        // �����ܻ�����  
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // ����Ƿ�����  
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        // ������������  
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // ������ײ��  
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // �����ƶ��� AI �ű�  
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // ֹͣ����ģ��  
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // ����Э�̵ȴ����������������  
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // ��ȡ����������ʱ��  
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
}