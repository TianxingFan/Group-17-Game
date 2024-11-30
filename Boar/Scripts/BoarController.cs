using UnityEngine;

public class BoarController : MonoBehaviour
{
    public float detectionRange = 10f;   // ������ҵķ�Χ  
    public float moveSpeed = 5f;          // ������ҵ��ٶ�  
    public float attackRange = 2f;        // ������Χ  
    public Animator animator;              // Animator���  
    public Transform player;               // ��� Transform  

    private bool isAttacking = false;

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // ��ʼ�������  
            animator.SetBool("isRunning", true);
            MoveTowardsPlayer(player.position);

            if (distanceToPlayer <= attackRange)
            {
                // �л�������״̬  
                StartAttack();
            }
        }
        else
        {
            // ���������Χ���ص�Idle״̬  
            animator.SetBool("isRunning", false);
        }
    }

    private void MoveTowardsPlayer(Vector2 playerPosition)
    {
        Vector2 direction = (playerPosition - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    private void StartAttack()
    {
        // �ڹ���״̬�·�ֹ�ظ�����  
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", true);
            // ���蹥������ʱ��Ϊ1��  
            Invoke("EndAttack", 1f);
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        // ���ر���״̬  
        animator.SetBool("isRunning", true);
    }
}