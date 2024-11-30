using UnityEngine;

public class PlayerHitBoxFlip : MonoBehaviour
{
    private BoxCollider2D attackCollider;
    private SpriteRenderer playerSprite;
    private float originalOffsetX;

    void Awake()
    {
        attackCollider = GetComponent<BoxCollider2D>();
        playerSprite = GetComponentInParent<SpriteRenderer>(); // 获取父物体(角色)的SpriteRenderer  
        if (attackCollider != null)
        {
            originalOffsetX = Mathf.Abs(attackCollider.offset.x); // 保存原始x偏移值的绝对值  
        }
    }

    void Update()
    {
        if (attackCollider != null && playerSprite != null)
        {
            Vector2 offset = attackCollider.offset;
            offset.x = originalOffsetX * (playerSprite.flipX ? -1 : 1); // 根据角色朝向调整偏移  
            attackCollider.offset = offset;
        }
    }
}