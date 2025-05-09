using UnityEngine;

public class Tornado : Enemy
{
    public void Launch(Vector2 velocity)
    {
        _rigidbody2D.linearVelocity = velocity;
    }
}