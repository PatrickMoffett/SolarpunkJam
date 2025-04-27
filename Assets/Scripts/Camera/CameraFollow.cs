using Services;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Time it takes to reach the target. Smaller = snappier.")]
    [SerializeField] private float smoothTime = 0.3f;

    [Tooltip("Half‐width and half‐height of the dead‐zone in world units.")]
    [SerializeField] private Vector2 deadZone = new Vector2(1f, 1f);
    [SerializeField] private Vector3 deadZoneOffset = new Vector3(0f, 0f, 0f);

    [SerializeField] private Vector2 maxDistance = new Vector2(5f, 5f);

    private Transform followTarget;
    private Vector3 currentVelocity;  // for SmoothDamp

    protected void Awake()
    {
        Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerFollowCamera(this);

    }
    protected void OnDestroy()
    {
        if (Services.ServiceLocator.Instance.Get<PlayerManager>().GetPlayerFollowCamera() == this)
        {
            Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerCharacter(null);
        }
    }

    public void SetDeadZoneOffset(Vector3 deadZoneOffset)
    {
        this.deadZoneOffset = deadZoneOffset;
    }
    public void SetDeadZone(Vector2 deadZone)
    {
        this.deadZone = deadZone;
    }
    public Vector2 GetDeadZone()
    {
        return deadZone;
    }
    public Vector3 GetDeadZoneOffset()
    {
        return deadZoneOffset;
    }
    void Start()
    {
        var playerCharacter = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter();
        if (playerCharacter != null)
        {
            followTarget = playerCharacter.transform;
        }
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Get<PlayerManager>().OnPlayerCharacterChanged += OnPlayerCharacterChanged;
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.Get<PlayerManager>().OnPlayerCharacterChanged -= OnPlayerCharacterChanged;
    }

    private void OnPlayerCharacterChanged(PlayerCharacter character)
    {
        followTarget = character != null ? character.transform : null;
    }

    void LateUpdate()
    {
        if (followTarget == null)
        {
            return;
        }
        Vector3 targetPos = CalculateTargetPosition();
        SmoothToTargetPosition(targetPos);
        ClampPositionToMaxDistanceFromTarget();
    }

    private void SmoothToTargetPosition(Vector3 targetPos)
    {
        // Smoothly move with SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            new Vector3(targetPos.x, targetPos.y, transform.position.z),
            ref currentVelocity,
            smoothTime
        );
    }

    private void ClampPositionToMaxDistanceFromTarget()
    {
        // After SmoothDamp, clamp against the same box you're drawing:
        Vector3 clampCenter = followTarget.position - deadZoneOffset;
        Vector3 halfExtents = new Vector3(
            maxDistance.x,
            maxDistance.y,
            0f
        );

        Vector3 clamped = transform.position;
        clamped.x = Mathf.Clamp(
            clamped.x,
            clampCenter.x - halfExtents.x,
            clampCenter.x + halfExtents.x
        );
        clamped.y = Mathf.Clamp(
            clamped.y,
            clampCenter.y - halfExtents.y,
            clampCenter.y + halfExtents.y
        );
        // preserve Z
        transform.position = new Vector3(clamped.x, clamped.y, transform.position.z);
    }

    private Vector3 CalculateTargetPosition()
    {
        // Figure out how far outside the dead-zone we are
        Vector3 delta = followTarget.position - deadZoneOffset - transform.position;
        Vector3 offset = Vector3.zero;
        if (Mathf.Abs(delta.x) > deadZone.x)
            offset.x = delta.x - Mathf.Sign(delta.x) * deadZone.x;
        if (Mathf.Abs(delta.y) > deadZone.y)
            offset.y = delta.y - Mathf.Sign(delta.y) * deadZone.y;

        // Compute the desired target position (preserve Z)
        Vector3 targetPos = transform.position + new Vector3(offset.x, offset.y, 0f);
        return targetPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float w = deadZone.x * 2f;
        float h = deadZone.y * 2f;
        Gizmos.DrawWireCube(transform.position + deadZoneOffset, new Vector3(w, h, 0f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + deadZoneOffset, new Vector3(maxDistance.x * 2f, maxDistance.y * 2f, 0f));

        if (followTarget == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(followTarget.position, 0.5f);
        Gizmos.DrawLine(followTarget.position, transform.position + deadZoneOffset);
    }
}
