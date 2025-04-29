using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AttributeSet))]
public class PlayerMovementComponent : MonoBehaviour
{    
    [Header("Movement Settings")]
    [SerializeField] private bool _airControl = false;              // Whether or not a player can steer while jumping;
    [SerializeField] private float _lowJumpGravityMultiplier = 2.5f;  // The gravity multiplier used when the player is holding the jump button
    [SerializeField] private float _maxFallSpeed = 20f;             // The maximum fall speed of the player
    [SerializeField] private float _coyoteTime = 0.1f;              // The time the player can jump after leaving the ground

    [Header("Collision Settings")]
    [SerializeField] private LayerMask _groundLayers;               // A mask determining what is ground to the character
    [SerializeField] private Transform _groundTransform;            // A position marking where to check if the player is grounded.
    [SerializeField] private float _groundedRadius = .2f;           // Radius of the overlap circle to determine if grounded
    [SerializeField] private Transform _ceilingTransform;           // A position marking where to check for ceilings
    [SerializeField] private float _ceilingRadius = .2f;            // Radius of the overlap circle to determine if the player can stand up
    [SerializeField] private float _jumpEpsilon = 0.1f; // The epsilon value used to determine if the player is jumping

    [Header("Knockback Settings")]
    [SerializeField] private float _knockbackSpeed = 10f;           // The force of the knockback
    [SerializeField] private float _knockbackTime = 0.2f;           // The time the player is knocked back

    [Header("Sound Effect")]
    [SerializeField] private SimpleAudioEvent _jumpAudioEvent;

    // Anim Variables
    private const string FALL_ANIM_TRIGGER = "Falling";
    private const string JUMP_ANIM_TRIGGER = "Jump";

    // Jump variables
    private bool _islowJumpGravityApplied = false;                  // Whether or not the low jump gravity multiplier is applied
    private bool _onGround;                                         // Whether or not the player is grounded.
    private bool _jumpPushed = false;                               // is the jump button pushed
    private bool _jumpWasTried = true;
    private bool _jumpedToLeaveGround = false;                      // Whether or not the player has jumped to leave the ground
    private bool _playerLeftGroundSinceLastJump = true;             // Whether or not the player has left the ground since the last jump
    private bool _coyoteJumpAvailable = false;                      // Whether or not the player can jump after leaving the ground
    private Coroutine _coyoteJumpCoroutine = null;                  // The coroutine that tracks the coyote jump time

    /// <summary>
    /// Public getter for checking if the player is on the ground or not
    /// </summary>
    public bool OnGround
    {
        get => _onGround;
    }
    
    // knockback variables
    private bool _knockbackActive = false;                         // Whether or not the player is currently knocked back

    // Direction variables
    private bool _facingRight = true;                               // For determining which way the player is currently facing.
    private Vector2 _movementDirection = Vector2.zero;

    // Components
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
        bool startedOnGround = _onGround;
        _onGround = checkIfOnGround();

        // if the Player has landed
        if (!startedOnGround && _onGround)
        {
            OnLandedOnGround();
        }
        else if(startedOnGround && !_onGround)
        {
            OnLeftTheGround();
        }
    }
    private void Update()
    {
        Move();
        UpdateCharacterAnim();
    }
    public void Move()
    {
        UpdateCharacterDirection();
        if (!_knockbackActive)
        {
            UpdateXVelocity();
            UpdateYVelocity();
        }
    }
    private void UpdateXVelocity()
    {
        float moveDirectionX = _movementDirection.x;

        Vector2 currentVelocity = Vector2.zero;

        if (_onGround)
        {
            ApplyGroundControl(moveDirectionX);
        }
        else if (_airControl)
        {
            ApplyAirControl(moveDirectionX);
        }


    }
    private void UpdateYVelocity()
    {
        // If the player should jump...
        if (ShouldJump())
        {
            PerformJump();
        }
        else if (_rigidbody2D.linearVelocity.y > _jumpEpsilon && _jumpedToLeaveGround && !_jumpPushed && !_islowJumpGravityApplied)
        {
            // If the player released the jump button early and is still going up,
            // use extra gravity to cut the jump height
            _rigidbody2D.gravityScale *= _lowJumpGravityMultiplier;
            _islowJumpGravityApplied = true;
        }
        if (_rigidbody2D.linearVelocity.y < -_maxFallSpeed)
        {
            _rigidbody2D.linearVelocityY = -_maxFallSpeed;
        }
        _jumpWasTried = true;
    }
    private void UpdateCharacterDirection()
    {
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
    private void UpdateCharacterAnim()
    {
        if (_rigidbody2D.linearVelocityY > _jumpEpsilon)
        {
            // Don't set the bool again if its already set
            if (_anim.GetBool(JUMP_ANIM_TRIGGER))
            {
                return;
            }
            _anim.SetBool(JUMP_ANIM_TRIGGER, true);
            _anim.SetBool(FALL_ANIM_TRIGGER, false);
        }
        else if (_rigidbody2D.linearVelocityY < -_jumpEpsilon && !_onGround)
        {
            // Don't set the bool again if its already set
            if (_anim.GetBool(FALL_ANIM_TRIGGER))
            {
                return;
            }
            _anim.SetBool(JUMP_ANIM_TRIGGER, false);
            _anim.SetBool(FALL_ANIM_TRIGGER, true);
        }
        else
        {
            _anim.SetBool(JUMP_ANIM_TRIGGER, false);
            _anim.SetBool(FALL_ANIM_TRIGGER, false);
        }
    }
    private void OnLeftTheGround()
    {
        // we reset _canJump here so that we can jump again
        // if we reset when we first jump, sometimes we jump twice
        // because we are still colliding with the ground
        _playerLeftGroundSinceLastJump = true;

        if (!_jumpedToLeaveGround)
        {
            _coyoteJumpCoroutine = StartCoroutine(TrackCoyoteTime());
        }
    }
    IEnumerator TrackCoyoteTime()
    {
        _coyoteJumpAvailable = true;
        yield return new WaitForSeconds(_coyoteTime);
        _coyoteJumpAvailable = false;
    }
    private void OnLandedOnGround()
    {
        // set the anim value
        _anim.SetBool(FALL_ANIM_TRIGGER, false);
        _jumpedToLeaveGround = false;

        // Reset the gravity scale to normal
        if (_islowJumpGravityApplied)
        {
            _rigidbody2D.gravityScale /= _lowJumpGravityMultiplier;
            _islowJumpGravityApplied = false;
        }

        StopCoyoteTime();
    }
    private void StopCoyoteTime()
    {
        // Stop the coyote jump coroutine if it is running
        if (_coyoteJumpCoroutine != null)
        {
            StopCoroutine(_coyoteJumpCoroutine);
            _coyoteJumpCoroutine = null;
            _coyoteJumpAvailable = false;
        }
    }
    private bool checkIfOnGround()
    {
        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundTransform.position, _groundedRadius, _groundLayers);
        for (int i = 0; i < colliders.Length; i++)
        {
            // if we collide with anything but ourselve, we are on the ground
            if (colliders[i].gameObject != gameObject) 
            {
                return true;
            }
        }
        return false;
    }
    private bool ShouldJump()
    {
        return (_onGround || _coyoteJumpAvailable) && _jumpPushed && !_jumpWasTried && _playerLeftGroundSinceLastJump;
    }
    private void PerformJump()
    {
        _jumpAudioEvent.Play(gameObject);

        if (_coyoteJumpAvailable)
        {
            StopCoyoteTime();
        }
        else
        { 
            _playerLeftGroundSinceLastJump = false;
        }
        _jumpedToLeaveGround = true;

        // Get the jump height from the attribute set
        Attribute jumpHeight = _attributeSet.GetAttribute(GlobalAttributes.JumpHeightAttribute);
        Assert.IsNotNull(jumpHeight, $"JumpHeight attribute not found: {GlobalAttributes.JumpHeightAttribute}");

        // Calculate the jump force to reach the jumpHeight
        float gravity = Physics2D.gravity.y * _rigidbody2D.gravityScale;
        float jumpForce = Mathf.Sqrt(-2f * gravity * jumpHeight.CurrentValue);
        // Add a vertical force to the player.
        _rigidbody2D.linearVelocityY = jumpForce;
    }
    private void ApplyGroundControl(float moveDirectionX)
    {
        Vector2 currentVelocity = _rigidbody2D.linearVelocity;
        Attribute movementAccel = _attributeSet.GetAttribute(GlobalAttributes.MoveAccelerationAttribute);
        Assert.IsNotNull(movementAccel, $"Move Acceleration attribute not found: {GlobalAttributes.MoveAccelerationAttribute}");
        
        Attribute movementSpeed = _attributeSet.GetAttribute(GlobalAttributes.MoveSpeedAttribute);
        Assert.IsNotNull(movementSpeed, $"Movement speed attribute not found: {GlobalAttributes.MoveSpeedAttribute}");

        if (moveDirectionX == 0f)
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

        // Apply the speed to the player
        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -movementSpeed.CurrentValue, movementSpeed.CurrentValue);
        currentVelocity.y = _rigidbody2D.linearVelocity.y;
        _rigidbody2D.linearVelocity = currentVelocity;
    }
    private void ApplyAirControl(float moveDirectionX)
    {
        Vector2 currentVelocity = _rigidbody2D.linearVelocity;
        Attribute airAccel = _attributeSet.GetAttribute(GlobalAttributes.AirControlAccelerationAttribute);
        Assert.IsNotNull(airAccel, $"Air Control attribute not found: {GlobalAttributes.AirControlAccelerationAttribute}");

        Attribute movementSpeed = _attributeSet.GetAttribute(GlobalAttributes.MoveSpeedAttribute);
        Assert.IsNotNull(movementSpeed, $"Movement speed attribute not found: {GlobalAttributes.MoveSpeedAttribute}");

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

        // Apply the speed to the player
        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -movementSpeed.CurrentValue, movementSpeed.CurrentValue);
        currentVelocity.y = _rigidbody2D.linearVelocity.y;
        _rigidbody2D.linearVelocity = currentVelocity;
    }
    internal void SetJumpPushed(bool jump)
    {
        _jumpWasTried = false;
        _jumpPushed = jump;
    }
    internal void SetMoveDirection(Vector2 moveDirection)
    {
        _movementDirection = moveDirection;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(_groundTransform.position, _groundedRadius);
    }

    public void ApplyKnockback(Vector3 knockbackDirection)
    {
        _rigidbody2D.linearVelocity = knockbackDirection * _knockbackSpeed;
        StartCoroutine(KnockbackCoroutine());
        _knockbackActive = true;
    }

    private IEnumerator KnockbackCoroutine()
    {
        yield return new WaitForSeconds(_knockbackTime);
        StopKnockback();
    }

    public void StopKnockback()
    {
        _knockbackActive = false;
    }


}