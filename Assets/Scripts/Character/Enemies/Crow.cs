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

    [Header("Patrol Settings")]
    public float patrolDistance = 10f;
    public float offscreenMargin = 2f;

    [Header("Gizmo Preview")]
    public float previewDistance = 5f;
    [Range(4, 100)]
    public int previewSegments = 30;

    // runtime state
    private Vector3 _basePosition;
    private Vector3 _turnOrigin;    
    private float _bobTime;
    private float _distanceSinceTurn;
    private bool _turnPending;
    private Camera _cam;

    protected override void Start()
    {
        base.Start();
        _basePosition = transform.position;
        _turnOrigin = _basePosition;
        flightDirection = flightDirection.normalized;
        _bobTime = 0f;
        _cam = Camera.main;
    }

    void Update()
    {
        _bobTime += Time.deltaTime;

        if (!_turnPending)
        {
            _distanceSinceTurn += flightSpeed * Time.deltaTime;
            if (_distanceSinceTurn >= patrolDistance)
                _turnPending = true;
        }
        else
        {
            //if (IsOffscreenBeyondMargin())
            {
                // reverse and reset
                flightDirection = -flightDirection;
                _distanceSinceTurn = 0f;
                transform.transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                _turnPending = false;

                // mark new patrol leg start
                _turnOrigin = _basePosition;
            }
        }

        // move base position
        _basePosition += (Vector3)flightDirection * flightSpeed * Time.deltaTime;

        // apply bobbing on top
        float bobOffset = Mathf.Sin(_bobTime * bobFrequency * 2f * Mathf.PI + bobPhase)
                          * bobAmplitude;
        transform.position = _basePosition + Vector3.up * bobOffset;
    }

    bool IsOffscreenBeyondMargin()
    {
        if (_cam == null) return false;

        if (!_cam.orthographic)
        {
            Vector3 vp = _cam.WorldToViewportPoint(_basePosition);
            return vp.x < -offscreenMargin || vp.x > 1f + offscreenMargin;
        }
        else
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;
            float camX = _cam.transform.position.x;
            return _basePosition.x < camX - halfW - offscreenMargin
                || _basePosition.x > camX + halfW + offscreenMargin;
        }
    }

    void OnDrawGizmosSelected()
    {
        // origin for bob preview
        Vector3 origin = Application.isPlaying ? _basePosition : transform.position;
        // origin for patrol-distance line
        Vector3 patrolOrigin = Application.isPlaying ? _turnOrigin : transform.position;
        Vector3 dir3 = new Vector3(flightDirection.x, flightDirection.y, 0f).normalized;
        float sampleBobTime = Application.isPlaying ? _bobTime : 0f;

        // --- sine-wave preview (cyan) ---
        Gizmos.color = Color.cyan;
        Vector3 last = origin;
        for (int i = 1; i <= previewSegments; i++)
        {
            float t = (float)i / previewSegments;
            float d = previewDistance * t;
            float futureTime = sampleBobTime + d / Mathf.Max(Mathf.Epsilon, flightSpeed);
            float offset = Mathf.Sin(futureTime * bobFrequency * 2f * Mathf.PI + bobPhase)
                               * bobAmplitude;

            Vector3 pt = origin + dir3 * d + Vector3.up * offset;
            Gizmos.DrawLine(last, pt);
            last = pt;
        }

        // --- patrol-distance preview (magenta) ---
        Gizmos.color = Color.magenta;
        Vector3 endPoint = patrolOrigin + dir3 * patrolDistance;
        Gizmos.DrawLine(patrolOrigin, endPoint);
        Gizmos.DrawWireSphere(patrolOrigin, 0.1f);
        Gizmos.DrawWireSphere(endPoint, 0.1f);

        // --- flight-direction arrow (yellow) ---
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + dir3);
        Vector3 right = Quaternion.LookRotation(Vector3.forward, dir3)
                          * Quaternion.Euler(0, 0, 160) * Vector3.up;
        Vector3 left = Quaternion.LookRotation(Vector3.forward, dir3)
                          * Quaternion.Euler(0, 0, -160) * Vector3.up;
        Gizmos.DrawLine(origin + dir3, origin + dir3 + right * 0.2f);
        Gizmos.DrawLine(origin + dir3, origin + dir3 + left * 0.2f);
    }
}