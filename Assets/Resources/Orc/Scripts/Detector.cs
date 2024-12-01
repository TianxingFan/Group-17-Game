using UnityEngine;

public class Detector : MonoBehaviour
{
    public GameObject DetectedPlayer { get; private set; }

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float detectionRadius = 5f;

    private OrcController orcController;

    private void Awake()
    {
        orcController = GetComponentInParent<OrcController>();
    }

    private void Start()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.radius = detectionRadius;
        }
        else
        {
            Debug.LogError("Detector needs a CircleCollider2D on the same GameObject!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && HasLineOfSight(other.transform))
        {
            DetectedPlayer = other.gameObject;
            orcController.OnPlayerDetected(DetectedPlayer);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (HasLineOfSight(other.transform))
            {
                DetectedPlayer = other.gameObject;
                orcController.OnPlayerDetected(DetectedPlayer);
            }
            else
            {
                DetectedPlayer = null;
                orcController.OnPlayerLost();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            DetectedPlayer = null;
            orcController.OnPlayerLost();
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);
        bool hasLineOfSight = hit.collider == null || hit.collider.CompareTag(playerTag);
        Debug.DrawLine(transform.position, target.position, hasLineOfSight ? Color.green : Color.red);

        return hasLineOfSight;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}