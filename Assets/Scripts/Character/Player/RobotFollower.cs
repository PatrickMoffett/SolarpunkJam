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

    private Vector3 _basePosition;
    private Vector3 _velocity = Vector3.zero;

    void Start()
    {
        Assert.IsNotNull(target, "Target Transform is not assigned in the inspector.");
        _basePosition = transform.position;
    }

    void Update()
    {
        if (target == null)
            return;

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
