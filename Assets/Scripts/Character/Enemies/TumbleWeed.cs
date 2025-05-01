using System;
using UnityEngine;
public class TumbleWeed : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] CollisionObserver2D collisionObserver;

    private Rigidbody2D _rb;
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
        collisionObserver.OnTriggerEnter += OnHitWall;
    }

    private void OnHitWall()
    {
        _enemy.Kill();
    }

    private void Update()
    {
        // maintain the speed
        _rb.linearVelocity = new Vector2(speed, _rb.linearVelocity.y);
    }
}