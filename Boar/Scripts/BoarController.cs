using UnityEngine;

public class BoarController : MonoBehaviour
{
    public float detectionRange = 10f;   // 发现玩家的范围  
    public float moveSpeed = 5f;          // 冲向玩家的速度  
    public float attackRange = 2f;        // 攻击范围  
    public Animator animator;              // Animator组件  
    public Transform player;               // 玩家 Transform  

    private bool isAttacking = false;

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // 开始冲向玩家  
            animator.SetBool("isRunning", true);
            MoveTowardsPlayer(player.position);

            if (distanceToPlayer <= attackRange)
            {
                // 切换到攻击状态  
                StartAttack();
            }
        }
        else
        {
            // 如果超出范围，回到Idle状态  
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
        // 在攻击状态下防止重复攻击  
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", true);
            // 假设攻击持续时间为1秒  
            Invoke("EndAttack", 1f);
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        // 返回奔跑状态  
        animator.SetBool("isRunning", true);
    }
}