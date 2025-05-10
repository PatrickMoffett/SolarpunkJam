#if UNITY_EDITOR
//#define MOVEMENT_COMPONENT_LOGGING
#endif
using Services;
using StateMachine;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AttributeSet))]
public class PlayerMovementComponent : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Settings")]
    [SerializeField] private bool _flipCameraOffset = true;       // Whether or not to flip the camera offset when the player flips
    [SerializeField] private bool _airControl = false;              // Whether or not a player can steer while jumping;
    [SerializeField] private CollisionObserver2D _groundObserver;   // The ground observer used to check if the player is on the ground
    [SerializeField] private float _lowJumpGravityMultiplier = 2.5f;// The gravity multiplier used when the player is holding the jump button
    [SerializeField] private Vector2 _maxSpeedEver = new Vector2(20f,20f);             // The maximum speed of the player
    [SerializeField] private float _coyoteTime = 0.1f;              // The time the player can jump after leaving the ground
    [SerializeField] private float _jumpEpsilon = 0.1f;             // The epsilon value used to determine if the player is jumping
    [SerializeField] private float _movementDampening = 100f;         // The amount of damping accel
    [SerializeField] private float _windSpeedModifier = .5f;
    [Header("JumpSlam Settings")]
    [SerializeField] private float _slamSpeed = 2f;                 // How fast to move the player as they are slamming
    [SerializeField] private GameEvent _slamStopEvent;              // The event to fire when the slam stops and the projectile should be destroyed
    

    
    [Header("Knockback Settings")]
    [SerializeField] private float _knockbackSpeed = 10f;           // The force of the knockback
    [SerializeField] private Vector2 _knockbackDirection = (Vector2.up + Vector2.left); // The direction of the knockback  
    [SerializeField] private float _knockbackTime = 0.2f;           // The time the player is knocked back

    [Header("Sound Effect")]
    [SerializeField] private SimpleAudioEvent _jumpAudioEvent;
    [SerializeField] private SimpleAudioEvent _jumpSlamAudioEvent;
    #endregion
    #region Private Variables
    private StateMachine<PlayerMovementComponent, BaseMovementState> _stateMachine;
    // Anim Variables
    private const string FALL_ANIM_TRIGGER = "Falling";
    private const string JUMP_ANIM_TRIGGER = "Jump";

    // Direction variables
    private bool _facingRight = true;                               // For determining which way the player is currently facing.
    private Vector2 _movementDirection = Vector2.zero;
    private Vector2 _cachedKnockbackVelocity = Vector2.zero; // The direction of the knockback
    private Vector2 _constantAccel = Vector2.zero; // The constant force to apply to the player
    #region Cached Components
    private Rigidbody2D _rigidbody2D;
    private Animator _anim;
    private AttributeSet _attributeSet;
    #endregion

    #region Cached Attributes
    Attribute _airAccel;
    Attribute _movementSpeed;
    Attribute _jumpHeight;
    Attribute _movementAccel;
    #endregion
    #endregion
    #region Public Interface
    public bool OnGround
    {
        get { return _stateMachine.CurrentState.GetType() == typeof(GroundState); }
    }
    public Vector2 GetMoveDirection()
    {
        return _movementDirection;
    }
    public void SetMoveDirection(Vector2 moveDirection)
    {
        _movementDirection = moveDirection;
    }
    public void ExecuteJump()
    {
        _stateMachine.CurrentState.ExecuteJump();
    }
    public void AbortJump()
    {
        _stateMachine.CurrentState.AbortJump();
    }
    public void ApplyKnockback(Vector2 knockbackDirection)
    {
        // TODO: Find a way to pass knockbackDirection to the KnockbackState
        _cachedKnockbackVelocity = _knockbackDirection * _knockbackSpeed;
        _cachedKnockbackVelocity.x *= Vector2.Dot(knockbackDirection, Vector2.left) >= 0f ? 1 : -1;

        _stateMachine.TransitionTo<KnockbackState>();
    }
    public void SetJumpSlamState()
    {
        _stateMachine.TransitionTo<JumpSlamState>();
    }
    public bool GetJumpSlamState()
    {
        return _stateMachine.CurrentState.GetType() == typeof(JumpSlamState);
    }
    public void AddConstantAcceleration(Vector2 acceleration)
    {
        _constantAccel += acceleration;
    }
    public void SubtractConstantAcceleration(Vector2 acceleration)
    {
        _constantAccel -= acceleration;
    }
    public void ResetConstantAcceleration()
    {
        _constantAccel = Vector2.zero;
    }
    #endregion
    #region Unity MonoBehaviour Callbacks
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _attributeSet = GetComponent<AttributeSet>();

        _stateMachine = new StateMachine<PlayerMovementComponent, BaseMovementState>(this);
        _stateMachine.TransitionTo<GroundState>();
    }
    private void OnEnable()
    {
        Assert.IsNotNull(_groundObserver, "Ground observer is not set");
        _groundObserver.OnTriggerEnter += OnGroundTriggerEnter;
        _groundObserver.OnTriggerExit += OnGroundTriggerExit;
    }
    private void OnDisable()
    {
        _groundObserver.OnTriggerEnter -= OnGroundTriggerEnter;
        _groundObserver.OnTriggerExit -= OnGroundTriggerExit;
    }
    private void Start()
    {
        _airAccel = _attributeSet.GetAttribute(GlobalAttributes.AirControlAccelerationAttribute);
        Assert.IsNotNull(_airAccel, $"Air Control attribute not found: {GlobalAttributes.AirControlAccelerationAttribute}");

        _movementSpeed = _attributeSet.GetAttribute(GlobalAttributes.MoveSpeedAttribute);
        Assert.IsNotNull(_movementSpeed, $"Movement speed attribute not found: {GlobalAttributes.MoveSpeedAttribute}");

        _jumpHeight = _attributeSet.GetAttribute(GlobalAttributes.JumpHeightAttribute);
        Assert.IsNotNull(_jumpHeight, $"JumpHeight attribute not found: {GlobalAttributes.JumpHeightAttribute}");

        _movementAccel = _attributeSet.GetAttribute(GlobalAttributes.MoveAccelerationAttribute);
        Assert.IsNotNull(_movementAccel, $"Move Acceleration attribute not found: {GlobalAttributes.MoveAccelerationAttribute}");
    }
    private void Update()
    {
        _stateMachine.CurrentState.Update();
        UpdateCharacterAnim();
    }
    #endregion
    #region Collision Callbacks
    private void OnGroundTriggerEnter(Collider2D collision)
    {
        if(_groundObserver.GetTriggerCount() == 1)
        {
            OnLandedOnGround();
        }
    }
    private void OnGroundTriggerExit(Collider2D collision)
    {
#if UNITY_EDITOR
        if(!gameObject.activeSelf)
        {
            return;
        }
#endif

        if(_groundObserver.GetTriggerCount() == 0)
        {
            OnLeftTheGround();
        }
    }

    private void OnLeftTheGround()
    {
        _stateMachine.CurrentState.OnLeftGround();
    }
    private void OnLandedOnGround()
    {
        _stateMachine.CurrentState.OnLandedOnGround();
    }
    #endregion
    #region Animation
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

        if (_flipCameraOffset) 
        { 
            var playerFollowCamera = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerFollowCamera();
            playerFollowCamera.SetDeadZoneOffset(new Vector3(playerFollowCamera.GetDeadZoneOffset().x * -1, playerFollowCamera.GetDeadZoneOffset().y, playerFollowCamera.GetDeadZoneOffset().z));
        }   
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
        else if (_rigidbody2D.linearVelocityY < -_jumpEpsilon)
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
    #endregion
    #region Logging
    public void Log(string message)
    {
#if MOVEMENT_COMPONENT_LOGGING
        Debug.Log($"[{gameObject.name}] {message}");
#endif
    }
    #endregion
    #region Motion Updates
    private void ClampFallSpeed()
    {
        if (_rigidbody2D.linearVelocity.y < -_maxSpeedEver.y)
        {
            _rigidbody2D.linearVelocityY = -_maxSpeedEver.y;
        }
    }
    private void AddJumpVelocity()
    {
        // Calculate the jump force to reach the jumpHeight
        float gravity = Physics2D.gravity.y * _rigidbody2D.gravityScale;
        float jumpForce = Mathf.Sqrt(-2f * gravity * _jumpHeight.CurrentValue);
        // Add a vertical force to the player.
        _rigidbody2D.linearVelocityY = jumpForce;
    }
    private void UpdateXVelocity(float moveDirectionX, float acceleration)
    {
        var v = _rigidbody2D.linearVelocity;
        float dt = Time.deltaTime;

        // 1) apply input + wind
        v.x += moveDirectionX * acceleration * dt
             + _constantAccel.x * dt;

        // 2) dampening when neither input nor wind
        if (moveDirectionX == 0f && Mathf.Abs(_constantAccel.x) < Mathf.Epsilon)
        {
            float brake = _movementDampening * dt;
            if (Mathf.Abs(v.x) < brake)
                v.x = 0f;
            else
                v.x -= Mathf.Sign(v.x) * brake;
        }

        // 3) choose your clamp based on wind vs. input
        bool windPresent = Mathf.Abs(_constantAccel.x) > Mathf.Epsilon;
        // windHelping: either no input (wind-only) or input in same dir as wind
        bool windHelping =
            windPresent
            && (moveDirectionX == 0f
                || Mathf.Sign(moveDirectionX) == Mathf.Sign(_constantAccel.x));

        if (windHelping)
        {
            // allow you to exceed your own move-speed, up to the absolute cap
            v.x = Mathf.Clamp(v.x, -_maxSpeedEver.x, _maxSpeedEver.x);
        }
        else
        {
            // if we're pushing into the wind lower max speed
            if(windPresent)
            {
                v.x = Mathf.Clamp(
                    v.x,
                    -_movementSpeed.CurrentValue * _windSpeedModifier,
                     _movementSpeed.CurrentValue * .7f
                );
            }
            else
            {
                v.x = Mathf.Clamp(
                    v.x,
                    -_movementSpeed.CurrentValue,
                     _movementSpeed.CurrentValue
                );
            }
        }

        // 4) write back
        v.y = _rigidbody2D.linearVelocity.y;
        _rigidbody2D.linearVelocity = v;
    }
    #endregion
    #region MovementStates
    private abstract class BaseMovementState : BaseState<PlayerMovementComponent>
    {
        public abstract void Update();
        public abstract void ExecuteJump();
        public abstract void AbortJump();
        public abstract void OnLeftGround();
        public abstract void OnLandedOnGround();
    }
    private class GroundState : BaseMovementState
    {
        public override void ExecuteJump()
        {
            Context._stateMachine.TransitionTo<JumpingState>();
        }

        public override void AbortJump() { }

        public override void EnterState(BaseState<PlayerMovementComponent> previousState)
        {
            // set the anim value
            Context._anim.SetBool(FALL_ANIM_TRIGGER, false);
        }

        public override void ExitState(BaseState<PlayerMovementComponent> nextState)
        {

        }

        public override void OnLandedOnGround()
        {

        }

        public override void OnLeftGround()
        { 
            Context._stateMachine.TransitionTo<FallingState>();
        }

        public override void Update()
        {
            Context.UpdateCharacterDirection();
            Context.UpdateXVelocity(Context._movementDirection.x,Context._movementAccel.CurrentValue);
            Context.UpdateCharacterAnim();
        }
    }
    private class JumpingState : BaseMovementState
    {
        bool _islowJumpGravityApplied = false;
        public override void ExecuteJump()
        {
        }
        public override void AbortJump()
        {
            if(!_islowJumpGravityApplied)
            {
                Context._rigidbody2D.gravityScale *= Context._lowJumpGravityMultiplier;
                _islowJumpGravityApplied = true;
            }
        }
        public override void EnterState(BaseState<PlayerMovementComponent> previousState)
        {
            Context._jumpAudioEvent.Play(Context.gameObject);
            Context.AddJumpVelocity();
        }
        public override void ExitState(BaseState<PlayerMovementComponent> nextState)
        {
            if (_islowJumpGravityApplied)
            {
                Context._rigidbody2D.gravityScale /= Context._lowJumpGravityMultiplier;
                _islowJumpGravityApplied = false;
            }
        }
        public override void OnLandedOnGround()
        {
            Context._stateMachine.TransitionTo<GroundState>();
        }
        public override void OnLeftGround()
        {
        }
        public override void Update()
        {
            Context.UpdateCharacterDirection();
            if (Context._airControl)
            {
                Context.UpdateXVelocity(Context._movementDirection.x, Context._airAccel.CurrentValue);
            }
            Context.ClampFallSpeed();
            Context.UpdateCharacterAnim();
        }
    }
    private class FallingState : BaseMovementState
    {
        public bool _coyoteJumpAvailable = false;
        private Coroutine _coyoteJumpCoroutine = null;
        public override void ExecuteJump()
        {
            if(_coyoteJumpAvailable)
            {
                _coyoteJumpAvailable = false;
                Context._stateMachine.TransitionTo<JumpingState>();
            }
        }
        public override void AbortJump()
        {
        }
        public override void EnterState(BaseState<PlayerMovementComponent> previousState)
        {
            // Don't provide coyote time jump if the player came from a knockback state
            // we don't want to allow the player to jump after being knocked back
            // because we don't know how long they might have been falling already
            if (previousState.GetType() != typeof(KnockbackState))
            {
                Context.StartCoroutine(TrackCoyoteTime());
            }
        }
        public override void ExitState(BaseState<PlayerMovementComponent> nextState)
        {
            if(_coyoteJumpCoroutine != null)
            {
                Context.StopCoroutine(_coyoteJumpCoroutine);
                _coyoteJumpCoroutine = null;
            }
        }
        public override void OnLandedOnGround()
        {
            Context._stateMachine.TransitionTo<GroundState>();
        }
        public override void OnLeftGround()
        {
        }
        public override void Update()
        {
            Context.UpdateCharacterDirection();
            if (Context._airControl)
            {
                Context.UpdateXVelocity(Context._movementDirection.x, Context._movementAccel.CurrentValue);
            }
            Context.ClampFallSpeed();
            Context.UpdateCharacterAnim();
        }
        IEnumerator TrackCoyoteTime()
        {
            _coyoteJumpAvailable = true;
            yield return new WaitForSeconds(Context._coyoteTime);
            _coyoteJumpAvailable = false;
        }
    }
    private class KnockbackState : BaseMovementState
    {
        Coroutine _knockbackCoroutine;
        public override void ExecuteJump()
        {
        }
        public override void AbortJump()
        {
        }
        public override void EnterState(BaseState<PlayerMovementComponent> previousState)
        {
            Context._rigidbody2D.linearVelocity = Context._cachedKnockbackVelocity;
            Context.StartCoroutine(KnockbackCoroutine());
        }
        public override void ExitState(BaseState<PlayerMovementComponent> nextState)
        {
            if(_knockbackCoroutine != null)
            {
                Context.StopCoroutine(_knockbackCoroutine);
                _knockbackCoroutine = null;
            }
        }
        public override void OnLandedOnGround()
        {
        }
        public override void OnLeftGround()
        {
        }
        public override void Update()
        {
        }
        private IEnumerator KnockbackCoroutine()
        {
            yield return new WaitForSeconds(Context._knockbackTime);
            OnKnockbackComplete();
        }
        public void OnKnockbackComplete()
        {
            if(Context._groundObserver.IsCurrentlyTriggering())
            {
                Context._stateMachine.TransitionTo<GroundState>();
                Context.Log("Knockback complete, transitioning to GroundState");
            }
            else
            {
                Context._stateMachine.TransitionTo<FallingState>();
                Context.Log("Knockback complete, transitioning to FallingState");
            }
        }
    }
    private class JumpSlamState : BaseMovementState
    {
        public override void EnterState(BaseState<PlayerMovementComponent> previousState)
        {
            // Keep the variable positive, so invert here
            if (Context.OnGround)
            {
                Context._stateMachine.TransitionTo<GroundState>();
            }
            Context._rigidbody2D.linearVelocityY = -Context._slamSpeed;
            Context._rigidbody2D.linearVelocityX = 0;
        }

        public override void ExitState(BaseState<PlayerMovementComponent> nextState)
        {
            Context._jumpSlamAudioEvent.Play(Context.gameObject);
            Context._slamStopEvent.Raise();
        }

        public override void Update()
        {
            
        }

        public override void ExecuteJump()
        {
            
        }

        public override void AbortJump()
        {
            
        }

        public override void OnLeftGround()
        {
            
        }

        public override void OnLandedOnGround()
        {
            Vector2 direction = Context._facingRight ? Vector2.left : Vector2.right;
            Context.ApplyKnockback(direction);
        }
    }
    #endregion
}