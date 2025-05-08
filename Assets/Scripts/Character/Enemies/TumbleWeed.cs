using UnityEngine;
using UnityEngine.Assertions;
public class TumbleWeed : Enemy
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] CollisionObserver2D wallCollisionObserver;
    [SerializeField] private GameObject deathEffect;
    private Rigidbody2D _rb;

    private static GameObject _vfxBucket;
    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();

        Assert.IsNotNull(_rb, $"Rigidbody2D not found on {gameObject.name}.");
        Assert.IsNotNull(wallCollisionObserver, $"CollisionObserver2D not found on {gameObject.name}.");
        Assert.IsNotNull(deathEffect, $"Death effect not assigned in {gameObject.name}.");
    }

    protected override void Start()
    {
        base.Start();

        if(_vfxBucket == null)
        {
            _vfxBucket = new GameObject("VFXBucket");
        }
        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        wallCollisionObserver.OnTriggerEnter += OnHitWall;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        wallCollisionObserver.OnTriggerEnter -= OnHitWall;
    }
    protected override void Die()
    {
        base.Die();
        // if object is on camera
        if (Camera.main != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            if (screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1)
            {
                // play the death effect
                PlayDeathEffect();
            }
        }
    }

    private void PlayDeathEffect()
    {
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        // add the effect to the bucket
        effect.transform.parent = _vfxBucket.transform;
    }

    private void OnHitWall(Collider2D collision)
    {
        Die();
    }

    private void Update()
    {
        // maintain the speed
        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
    }
}