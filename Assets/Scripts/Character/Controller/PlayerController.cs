using System;
using System.Collections;
using System.Collections.Generic;
using Abilities;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementComponent))]
public class PlayerController : MonoBehaviour
{
    private const string SHOOT_ANIM_TRIGGER = "Shoot";
    private const string PUNCH_ANIM_TRIGGER = "Punch";
    private const string BOSS_ANIM_TRIGGER = "BossAbility";
    private const string DAMAGED_ANIM_TRIGGER = "Hit";
    private const string JUMP_SLAM_ANIM_TRIGGER = "JumpSlam";
    private const string WALK_ANIM_BOOL = "Walking";
    private const string START_WALK_ANIM_TRIGGER = "StartWalking";
    private const string STOP_WALK_ANIM_TRIGGER = "StopWalking";
    private const string CHARGE_ANIM_BOOL = "Charging";
    
    private InputSystem_Actions _input;
    private PlayerMovementComponent _movementComponent;
    private AttributeSet _attributes;

    enum PlayerControllerState
    {
        Default,
        Dialogue,
        Menu
    }
    private PlayerControllerState _controllerState = PlayerControllerState.Default;

    [Tooltip("The player's health Attribute, used to track damage and trigger the right animations")]
    [SerializeField] private AttributeType healthAttribute;
    
    [Header("Attack information")]
    [Tooltip("The various shoot attacks the player currently has access to")]
    [SerializeField] private List<Ability> _shootAttacks;

    [Tooltip("The various punch attacks the player currently has access to")]
    [SerializeField] private List<Ability> _punchAttacks;

    [Tooltip("The various boss skills the player currently has access to")]
    [SerializeField] private List<Ability> _bossAttacks;

    [Tooltip("The various jump/in-air attacks the player has")]
    [SerializeField] private List<Ability> _jumpAttacks;
    
    [Tooltip("How long thew player needs to hold the button down for a charge attack. Make negative to disable")]
    [SerializeField] private float shotChargeTime;
    
    [Tooltip("The charging VFX object to enable/disable based on player charge state")]
    [SerializeField] private GameObject chargeVFX;

    [Tooltip("The charging Icon to put over the Players Glove")]
    [SerializeField] private GameObject chargeIcon;

    [Header("Sound Effects")]
    [Tooltip("The sound to play when the charge timer is complete")]
    [SerializeField] private SimpleAudioEvent chargeSoundEvent;
    [Tooltip("The sound to play when the player takes damage")]
    [SerializeField] private SimpleAudioEvent damagedSoundEvent;
    [Tooltip("The sound to play when pausing and unpausing")]
    [SerializeField] private SimpleAudioEvent pauseSoundEvent;
    
    private Animator _animator;
    private bool isCharging = false;
    private float chargeTimeCounter = 0;
    private Vector2 attackDirection = Vector2.right;
    private void Awake()
    {
        Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerController(this);
        _animator = GetComponent<Animator>();

        _input = new InputSystem_Actions();
        _movementComponent = GetComponent<PlayerMovementComponent>();
        _attributes = GetComponent<AttributeSet>();

        // Jump
        _input.Player.Jump.started += OnJumpStarted;
        _input.Player.Jump.canceled += OnJumpCanceled;

        // Move
        _input.Player.Move.performed += OnMovePerformed;
        _input.Player.Move.canceled += OnMoveCanceled;

        // Attacks
        _input.Player.Shoot.performed += OnShootPerformed;
        _input.Player.Shoot.canceled += OnShootCanceled;
        _input.Player.Punch.canceled += OnPunchCanceled;
        _input.Player.BossSkill.canceled += OnBossSkillCanceled;

        // Dialogue
        _input.Dialogue.NextDialogue.performed += OnNextDialogue;
        _input.Dialogue.SkipDialogue.performed += OnSkipDialogue;
        _input.Dialogue.SpeedUpDialogue.started += OnSpeedUpStarted;
        _input.Dialogue.SpeedUpDialogue.canceled += OnSpeedUpCanceled;

        _input.Universal.Pause.performed += ToggleMenu;

        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueStart += OnDialogueStart;
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd += OnDialogueEnd;
    }
    private void OnEnable()
    {
        _input.Enable();
        _input.Dialogue.Disable();

    }

    private void OnDisable()
    {
        _input.Disable();

        // Jump
        _input.Player.Jump.started -= OnJumpStarted;
        _input.Player.Jump.canceled -= OnJumpCanceled;

        // Move
        _input.Player.Move.performed -= OnMovePerformed;
        _input.Player.Move.canceled -= OnMoveCanceled;

        // Attacks
        _input.Player.Shoot.performed -= OnShootPerformed;
        _input.Player.Shoot.canceled -= OnShootCanceled;
        _input.Player.Punch.canceled -= OnPunchCanceled;
        _input.Player.BossSkill.canceled -= OnBossSkillCanceled;

        // Dialogue
        _input.Dialogue.NextDialogue.performed -= OnNextDialogue;
        _input.Dialogue.SkipDialogue.performed -= OnSkipDialogue;
        _input.Dialogue.SpeedUpDialogue.started -= OnSpeedUpStarted;
        _input.Dialogue.SpeedUpDialogue.canceled -= OnSpeedUpCanceled;

        // Menu
        _input.Universal.Pause.performed -= ToggleMenu;

        _input.Universal.Pause.performed -= ctx => ToggleMenu();
    }
    private void ToggleMenu(InputAction.CallbackContext ctx) => ToggleMenu();
    private void OnJumpStarted(InputAction.CallbackContext ctx) => ExecuteJump();
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => AbortJump();

    private void OnMovePerformed(InputAction.CallbackContext ctx) => Move(ctx.ReadValue<Vector2>());
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => Move(Vector2.zero);

    private void OnShootPerformed(InputAction.CallbackContext ctx) => StartAttack(shotChargeTime);
    private void OnShootCanceled(InputAction.CallbackContext ctx) =>
        EndAttack(_shootAttacks, SHOOT_ANIM_TRIGGER, shotChargeTime);

    private void OnPunchCanceled(InputAction.CallbackContext ctx) =>
        EndAttack(_punchAttacks, PUNCH_ANIM_TRIGGER);

    private void OnBossSkillCanceled(InputAction.CallbackContext ctx) =>
        EndAttack(_bossAttacks, BOSS_ANIM_TRIGGER);

    private void OnNextDialogue(InputAction.CallbackContext ctx) => NextDialogue();
    private void OnSkipDialogue(InputAction.CallbackContext ctx) => SkipDialogue();
    private void OnSpeedUpStarted(InputAction.CallbackContext ctx) => SpeedUpDialogue(true);
    private void OnSpeedUpCanceled(InputAction.CallbackContext ctx) => SpeedUpDialogue(false);


    private void ToggleMenu()
    {
        // No opening menu during dialogue (which also means no closing)
        // but since we can never open it with dialogue, that shouldn't be needed
        if(ServiceLocator.Instance.Get<DialogueSystem>().IsDialogueActive())
        {
            return;
        }
        
        if (pauseSoundEvent)
        {
            pauseSoundEvent.Play();
        }
        
        var currentState = ServiceLocator.Instance.Get<ApplicationStateManager>().GetCurrentState();
        if (currentState.GetType() == typeof(GameState))
        {
            ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<PauseMenuState>();
            return;
        }
        else
        {
            while (currentState.GetType() != typeof(GameState))
            {
                ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
                currentState = ServiceLocator.Instance.Get<ApplicationStateManager>().GetCurrentState();
            }
        }

    }

    private void SpeedUpDialogue(bool shouldSpeedUp)
    {
        ServiceLocator.Instance.Get<DialogueSystem>().SetSpeedUpText(shouldSpeedUp);
    }

    private void SkipDialogue()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().ForceCompleteLine();
    }

    private void NextDialogue()
    {
        ServiceLocator.Instance.Get<DialogueSystem>().RequestNextLine();
    }
    private void OnDialogueEnd()
    {
        _input.Player.Enable();
        _input.Dialogue.Disable();
    }

    private void OnDialogueStart()
    {
        _input.Dialogue.Enable();
        _input.Player.Disable(); 
    }

    private void Start()
    {
        _attributes.GetAttribute(healthAttribute).OnValueChanged += OnHealthChanged;
    }

    protected void OnDestroy()
    {
        if (Services.ServiceLocator.Instance.Get<PlayerManager>().GetPlayerController() == this)
        {
            Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerController(null);
        }
    }
    private void ExecuteJump()
    {
        _movementComponent.ExecuteJump();
    }
    private void AbortJump()
    {
        _movementComponent.AbortJump();
    }
    private void Move(Vector2 vector2)
    {
        _movementComponent.SetMoveDirection(vector2);
        if (vector2 != Vector2.zero)
        {
            attackDirection = vector2;
        }

        if (vector2.x != 0)
        {
            if (!_animator.GetBool(WALK_ANIM_BOOL))
            {
                _animator.ResetTrigger(STOP_WALK_ANIM_TRIGGER);
                _animator.SetTrigger(START_WALK_ANIM_TRIGGER);
            }

            _animator.SetBool(WALK_ANIM_BOOL, true);
        }
        else
        {
            if (_animator.GetBool(WALK_ANIM_BOOL))
            {
                _animator.ResetTrigger(START_WALK_ANIM_TRIGGER);
                _animator.SetTrigger(STOP_WALK_ANIM_TRIGGER);
            }
            _animator.SetBool(WALK_ANIM_BOOL, false);
        }
        // Jump slam attack
        if (vector2.y < 0 && !_movementComponent.GetJumpSlamState() && !_movementComponent.OnGround)
        {
            _movementComponent.SetJumpSlamState();
            attackDirection = Vector2.down;
            _movementComponent.SetMoveDirection(Vector2.down);
            EndAttack(_jumpAttacks, JUMP_SLAM_ANIM_TRIGGER);
        }
    }
    
    private void OnHealthChanged(Attribute attr, float previousValue)
    {
        // Don't play the damaged animation if the change isn't damage
        if (attr.CurrentValue >= previousValue)
        {
            return;
        }

        if (damagedSoundEvent)
        {
            damagedSoundEvent.Play(gameObject);
        }
        _animator.SetTrigger(DAMAGED_ANIM_TRIGGER);
    }

    // TODO: Actually differentiate attacks based on button pressed/time pressed/etc...
    private void StartAttack(float chargeTime)
    {
        isCharging = true;
        chargeVFX.SetActive(true);
        chargeIcon.SetActive(true);
        //_animator.SetBool(CHARGE_ANIM_BOOL, true);
        StartCoroutine(IncrementCharge(chargeTime));
    }

    private void EndAttack(List<Ability> abilities, string animationTrigger, float chargeTime = -1)
    {
        AbilityTargetData target = new AbilityTargetData
        {
            sourceCharacterLocation = transform.position,
            sourceCharacterDirection = attackDirection,
            animationTrigger = animationTrigger
        };
        Ability chosenAttack = abilities[0];
        if (chargeTime > 0)
        {
            isCharging = false;
            if (chargeTimeCounter >= chargeTime)
            {
                Debug.Log($"[{GetType().Name}] Charged attack firing!");
                chosenAttack = abilities[1];
            }
            chargeTimeCounter = 0;
        }

        chargeVFX.SetActive(false);
        chargeIcon.SetActive(false);
        //_animator.SetBool(CHARGE_ANIM_BOOL, false);
        chosenAttack.Initialize(gameObject);
        chosenAttack.TryActivate(target);
    }

    private IEnumerator IncrementCharge(float chargeTime)
    {
        while (isCharging)
        {
            chargeTimeCounter += Time.deltaTime;
            if (chargeTimeCounter >= chargeTime)
            {
                if (chargeSoundEvent)
                {
                    chargeSoundEvent.Play(gameObject);
                }
                break;
            }
            yield return null;
        }
    }
}