using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Patrol2D : Enemy
{
    [Header("Patrol Points (Children in Prefab)")]
    [Tooltip("These will be un‐parented at runtime so they don’t move with the AI.")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Seconds to wait at each end before turning.")]
    [SerializeField] private float waitTime = 1f;
    [Tooltip("How close in X you must be to count as 'arrived'.")]
    [SerializeField] private float pointThreshold = 0.1f;

    const string ANIM_WALK = "Walking";

    private Rigidbody2D rb;
    private Transform currentTarget;

    private static GameObject _patrolPointBucket;
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

        if(_patrolPointBucket == null)
        {
            _patrolPointBucket = new GameObject("Patrol Points");
        }
        // Detach the markers so they keep their world positions
        if (pointA) pointA.SetParent(_patrolPointBucket.transform, true);
        if (pointB) pointB.SetParent(_patrolPointBucket.transform, true);
    }

    protected override void Start()
    {
        base.Start();
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Patrol2D: Please assign Point A and Point B in the inspector.");
            enabled = false;
            return;
        }

        currentTarget = pointB;

        StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if(!_isDying)
            {
                _animator.SetBool(ANIM_WALK, true);
            }
            // Move on X toward the current target
            while (Mathf.Abs(rb.position.x - currentTarget.position.x) > pointThreshold)
            {
                if(_isDying)
                {
                    break; ; // stop if we are dying
                }
                float dir = Mathf.Sign(currentTarget.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
                // Re-lock Y in case of any drift
                rb.position = new Vector2(rb.position.x, rb.position.y);
                yield return new WaitForFixedUpdate();
            }

            // Arrived: stop, lock X to target, wait
            _animator.SetBool(ANIM_WALK, false);
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(waitTime);

            // Flip target and repeat
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA && pointB)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, pointThreshold);
            Gizmos.DrawSphere(pointB.position, pointThreshold);
            Gizmos.DrawLine(
                pointA.position,
                pointB.position);
        }
    }
    private void Update()
    {
        // flip sprite based on dir
        if (currentTarget != null)
        {
            Vector2 toTarget = (currentTarget.position - transform.position);
            if (toTarget.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
