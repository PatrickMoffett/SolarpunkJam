using UnityEngine;
using UnityEngine.Assertions;

public class RobotFollower : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float maxSpeed = 5f;

    [Header("Bobbing")]
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobFrequency = 1f;

    [Header("Direction")]
    private bool startFacingRight = true;

    private Vector3 _basePosition;
    private Vector3 _velocity = Vector3.zero;
    private bool lookingRight = true;

    void Start()
    {
        Assert.IsNotNull(target, "Target Transform is not assigned in the inspector.");
        _basePosition = transform.position;
        if (!startFacingRight)
        {
            lookingRight = false;
        }
    }

    void Update()
    {
        if (target == null)
            return;

        Move();
        UpdateDirection();
    }

    private void UpdateDirection()
    {
        if ((lookingRight && target.position.x < transform.position.x)
            || (!lookingRight && target.position.x > transform.position.x))
        {
            lookingRight = !lookingRight;
            transform.localScale = new Vector3(
                -transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z);
        }
    }

    private void Move()
    {
        // move towards target
        _basePosition = Vector3.SmoothDamp(
            _basePosition,
            target.position,
            ref _velocity,
            smoothTime,
            maxSpeed
        );

        // sin wave bobbing
        float bobOffset = Mathf.Sin(Time.time * bobFrequency * 2f * Mathf.PI) * bobHeight;

        // Apply both
        transform.position = _basePosition + Vector3.up * bobOffset;
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
