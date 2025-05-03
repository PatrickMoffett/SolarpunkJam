using UnityEngine;

public class Crow : Enemy
{
    [Header("Flight Settings")]
    public Vector2 flightDirection = Vector2.right;
    public float flightSpeed = 2f;

    [Header("Bobbing Settings")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 1f;
    public float bobPhase = 0f;

    [Header("Gizmo Preview")]
    public float previewDistance = 5f;
    [Range(4, 100)]
    public int previewSegments = 30;

    // Remember the starting position (set at runtime)
    private Vector3 _startPos;
    private float _distanceTraveled = 0f;
    private float _bobTime = 0f;

    protected override void Start()
    {
        base.Start();
        _startPos = transform.position;
        flightDirection = flightDirection.normalized;
    }

    void Update()
    {
        // Advance the bird's own bob‐timer
        _bobTime += Time.deltaTime;

        // Move forward
        _distanceTraveled += flightSpeed * Time.deltaTime;
        Vector3 forwardOffset = (Vector3)flightDirection * _distanceTraveled;

        // Compute bob offset using the bird's own time
        float bobOffset = Mathf.Sin((_bobTime * bobFrequency * 2f * Mathf.PI) + bobPhase)
                          * bobAmplitude;
        Vector3 verticalOffset = Vector3.up * bobOffset;

        // Apply combined motion
        transform.position = _startPos + forwardOffset + verticalOffset;
    }

    void OnDrawGizmosSelected()
    {
        // Base point for the preview (use current position in editor)
        Vector3 origin = Application.isPlaying ? _startPos : transform.position;
        Vector3 dir3 = new Vector3(flightDirection.x, flightDirection.y, 0f).normalized;

        Gizmos.color = Color.cyan;
        Vector3 lastPoint = origin;

        // Draw the sine?wave path by sampling along distance
        for (int i = 1; i <= previewSegments; i++)
        {
            float t = (float)i / previewSegments;
            float d = previewDistance * t;

            // Estimate the time at which the bird would reach distance d
            float sampleTime = d / Mathf.Max(Mathf.Epsilon, flightSpeed);

            // Compute the bob at that future time
            float bobOffset = Mathf.Sin(sampleTime * bobFrequency * 2f * Mathf.PI + bobPhase)
                              * bobAmplitude;

            Vector3 samplePoint = origin + dir3 * d + Vector3.up * bobOffset;
            Gizmos.DrawLine(lastPoint, samplePoint);
            lastPoint = samplePoint;
        }

        // Also draw an arrow for flight direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + dir3);
        // Arrowhead
        Vector3 right = Quaternion.LookRotation(Vector3.forward, dir3) * Quaternion.Euler(0, 0, 160) * Vector3.up;
        Vector3 left = Quaternion.LookRotation(Vector3.forward, dir3) * Quaternion.Euler(0, 0, -160) * Vector3.up;
        Gizmos.DrawLine(origin + dir3, origin + dir3 + right * 0.2f);
        Gizmos.DrawLine(origin + dir3, origin + dir3 + left * 0.2f);
    }
}