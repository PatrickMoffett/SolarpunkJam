using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AttributeSet))]
public class PlayerMovementComponent : MonoBehaviour
{

    private const string FALL_ANIM_TRIGGER = "Falling";
    private const string JUMP_ANIM_TRIGGER = "Jump";
    
    [Header("Movement Settings")]
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
    private Animator _anim;
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
        _anim = GetComponent<Animator>();
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
                    _anim.SetBool(FALL_ANIM_TRIGGER, false);
                }
                return;
            }
        }
        // The player is not grounded or we would have returned
        _canJump = true;

        if (_rigidbody2D.linearVelocityY > 0)
        {
            _anim.SetBool(JUMP_ANIM_TRIGGER, true);
            _anim.SetBool(FALL_ANIM_TRIGGER, false);
        }
        else if (_rigidbody2D.linearVelocityY < 0 && !_onGround)
        {
            _anim.SetBool(JUMP_ANIM_TRIGGER, false);
            _anim.SetBool(FALL_ANIM_TRIGGER, true);
        }
    }

    private void Update()
    {
        Move();
    }

    public void Move()
    {
        Attribute movementSpeed = _attributeSet.GetAttribute(GlobalAttributes.MoveSpeedAttribute);
        Assert.IsNotNull(movementSpeed, $"Movement speed attribute not found: {GlobalAttributes.MoveSpeedAttribute}");

        float moveDirectionX = _movementDirection.x;

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

        Vector2 currentVelocity = _rigidbody2D.linearVelocity;
        //only control the player if grounded or airControl is turned on
        if (_onGround)
        {
            Attribute movementAccel = _attributeSet.GetAttribute(GlobalAttributes.MoveAccelerationAttribute);
            Assert.IsNotNull(movementAccel, $"Move Acceleration attribute not found: {GlobalAttributes.MoveAccelerationAttribute}");
            if(moveDirectionX == 0f)
            {
                // If the player is not moving, we want to decelerate
                if (currentVelocity.x > 0f)
                {
                    currentVelocity.x -= movementAccel.CurrentValue * Time.deltaTime;
                    if (currentVelocity.x < 0f)
                    {
                        currentVelocity.x = 0f;
                    }
                }
                else if (currentVelocity.x < 0f)
                {
                    currentVelocity.x += movementAccel.CurrentValue * Time.deltaTime;
                    if (currentVelocity.x > 0f)
                    {
                        currentVelocity.x = 0f;
                    }
                }
            }
            else
            {
                currentVelocity.x += moveDirectionX * movementAccel.CurrentValue * Time.deltaTime;
            }
        }
        else if (_airControl)
        {
            Attribute airAccel = _attributeSet.GetAttribute(GlobalAttributes.AirControlAccelerationAttribute);
            Assert.IsNotNull(airAccel, $"Air Control attribute not found: {GlobalAttributes.AirControlAccelerationAttribute}");

            if (moveDirectionX == 0f)
            {
                // If the player is not moving, we want to decelerate
                if (currentVelocity.x > 0f)
                {
                    currentVelocity.x -= airAccel.CurrentValue * Time.deltaTime;
                    if (currentVelocity.x < 0f)
                    {
                        currentVelocity.x = 0f;
                    }
                }
                else if (currentVelocity.x < 0f)
                {
                    currentVelocity.x += airAccel.CurrentValue * Time.deltaTime;
                    if (currentVelocity.x > 0f)
                    {
                        currentVelocity.x = 0f;
                    }
                }
            }
            else
            {
                currentVelocity.x += moveDirectionX * airAccel.CurrentValue * Time.deltaTime;
            }
        }

        // Apply the speed to the player
        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -movementSpeed.CurrentValue, movementSpeed.CurrentValue);
        currentVelocity.y = _rigidbody2D.linearVelocity.y;
        _rigidbody2D.linearVelocity = currentVelocity;

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