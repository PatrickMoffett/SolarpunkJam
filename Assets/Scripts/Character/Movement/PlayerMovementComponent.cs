using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AttributeSet))]
public class PlayerMovementComponent : MonoBehaviour
{

    [Range(0, .3f)][SerializeField] private float _movementSmoothing = .05f;   // How much to smooth out the movement
    [SerializeField] private bool _airControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask _groundLayers;                          // A mask determining what is ground to the character
    [SerializeField] private Transform _groundTransform;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform _ceilingTransform;                          // A position marking where to check for ceilings

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up

    private bool _onGround;            // Whether or not the player is grounded.
    private bool _jumpPushed = false;
    private bool _canJump = true;
    private bool _facingRight = true;  // For determining which way the player is currently facing.

    private Vector2 _movementDirection = Vector2.zero;
    private Vector3 _velocity = Vector3.zero;

    private Rigidbody2D _rigidbody2D;
    private AttributeSet _attributeSet;

    public Vector2 MoveDirection
    {
        get
        {
            return _movementDirection;
        }
    }
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _attributeSet = GetComponent<AttributeSet>();
    }

    private void FixedUpdate()
    {
        bool startedOnGrounded = _onGround;
        _onGround = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundTransform.position, k_GroundedRadius, _groundLayers);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                _onGround = true;
                if (!startedOnGrounded)
                {
                    // The Player has landed
                }
                return;
            }
        }
        // The player is not grounded or we would have returned
        _canJump = true;
    }

    private void Update()
    {
        Move();
    }

    public void Move()
    {
        float moveDirectionX = _movementDirection.x;
        //only control the player if grounded or airControl is turned on
        if (_onGround || _airControl)
        {
            // Get the movement speed from the attribute set
            Attribute movementSpeed = _attributeSet.GetAttribute(GlobalAttributes.MoveSpeedAttribute);
            Assert.IsNotNull(movementSpeed, $"Movement speed attribute not found: {GlobalAttributes.MoveSpeedAttribute}");
            //_attributeSet.GetAttribute.TryGetValue(_movementSpeedAttribute, out var movementSpeed);
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(moveDirectionX * movementSpeed.CurrentValue, _rigidbody2D.linearVelocity.y);
            // And then smoothing it out and applying it to the character
            _rigidbody2D.linearVelocity = Vector3.SmoothDamp(_rigidbody2D.linearVelocity, targetVelocity, ref _velocity, _movementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (moveDirectionX > 0 && !_facingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (moveDirectionX < 0 && _facingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should jump...
        if (_onGround && _jumpPushed && _canJump)
        {
            _onGround = false;
            _canJump = false;

            // Get the jump height from the attribute set
            Attribute jumpHeight = _attributeSet.GetAttribute(GlobalAttributes.JumpHeightAttribute);
            Assert.IsNotNull(jumpHeight, $"JumpHeight attribute not found: {GlobalAttributes.JumpHeightAttribute}");

            // Calculate the jump force to reach the jumpHeight
            float gravity = Physics2D.gravity.y * _rigidbody2D.gravityScale;
            float jumpForce = Mathf.Sqrt(-2f * gravity * jumpHeight.CurrentValue);
            // Add a vertical force to the player.
            _rigidbody2D.linearVelocityY = jumpForce;
        }
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        _facingRight = !_facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    internal void SetJump(bool jump)
    {
        _jumpPushed = jump;
    }

    internal void SetMoveDirection(Vector2 moveDirection)
    {
        _movementDirection = moveDirection;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_groundTransform.position, k_GroundedRadius);
    }
}