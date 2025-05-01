using UnityEngine;
using UnityEngine.Assertions;
public class TumbleWeed : Enemy
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] CollisionObserver2D collisionObserver;
    [SerializeField] private GameObject deathEffect;
    private Rigidbody2D _rb;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();

        Assert.IsNotNull(_rb, $"Rigidbody2D not found on {gameObject.name}.");
        Assert.IsNotNull(collisionObserver, $"CollisionObserver2D not found on {gameObject.name}.");
        Assert.IsNotNull(deathEffect, $"Death effect not assigned in {gameObject.name}.");
    }

    protected override void Start()
    {
        base.Start();

        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
    }

    private void OnEnable()
    {
        collisionObserver.OnTriggerEnter += OnHitWall;
    }
    private void OnDisable()
    {
        collisionObserver.OnTriggerEnter -= OnHitWall;
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
        Instantiate(deathEffect, transform.position, Quaternion.identity);
    }

    private void OnHitWall()
    {
        Die();
    }

    private void Update()
    {
        // maintain the speed
        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
    }
}