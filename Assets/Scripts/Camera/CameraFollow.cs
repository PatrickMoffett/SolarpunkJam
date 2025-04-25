using Services;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Time it takes to reach the target. Smaller = snappier.")]
    [SerializeField] private float smoothTime = 0.3f;

    [Tooltip("Half‐width and half‐height of the dead‐zone in world units.")]
    [SerializeField] private Vector2 deadZone = new Vector2(1f, 1f);

    private Transform followTarget;
    private Vector3 currentVelocity;  // for SmoothDamp

    void Start()
    {
        var playerCharacter = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter();
        if (playerCharacter != null)
            followTarget = playerCharacter.transform;
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
            return;

        // 1) Figure out how far outside the dead-zone we are
        Vector3 delta = followTarget.position - transform.position;
        Vector3 offset = Vector3.zero;
        if (Mathf.Abs(delta.x) > deadZone.x)
            offset.x = delta.x - Mathf.Sign(delta.x) * deadZone.x;
        if (Mathf.Abs(delta.y) > deadZone.y)
            offset.y = delta.y - Mathf.Sign(delta.y) * deadZone.y;

        // 2) Compute the desired target position (preserve Z)
        Vector3 targetPos = transform.position + new Vector3(offset.x, offset.y, 0f);

        // 3) Smoothly move with SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            new Vector3(targetPos.x, targetPos.y, transform.position.z),
            ref currentVelocity,
            smoothTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float w = deadZone.x * 2f;
        float h = deadZone.y * 2f;
        Gizmos.DrawWireCube(transform.position, new Vector3(w, h, 0f));
    }
}
