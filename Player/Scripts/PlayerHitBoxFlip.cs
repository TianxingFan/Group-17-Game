using UnityEngine;

public class PlayerHitBoxFlip : MonoBehaviour
{
    private BoxCollider2D attackCollider;
    private SpriteRenderer playerSprite;
    private float originalOffsetX;

    void Awake()
    {
        attackCollider = GetComponent<BoxCollider2D>();
        playerSprite = GetComponentInParent<SpriteRenderer>(); // ��ȡ������(��ɫ)��SpriteRenderer  
        if (attackCollider != null)
        {
            originalOffsetX = Mathf.Abs(attackCollider.offset.x); // ����ԭʼxƫ��ֵ�ľ���ֵ  
        }
    }

    void Update()
    {
        if (attackCollider != null && playerSprite != null)
        {
            Vector2 offset = attackCollider.offset;
            offset.x = originalOffsetX * (playerSprite.flipX ? -1 : 1); // ���ݽ�ɫ�������ƫ��  
            attackCollider.offset = offset;
        }
    }
}