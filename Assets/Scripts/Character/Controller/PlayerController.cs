using System;
using System.Collections;
using System.Collections.Generic;
using Abilities;
using Services;
using UnityEngine;

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

        _input.Player.Jump.started += ctx => ExecuteJump();
        _input.Player.Jump.canceled += ctx => AbortJump();
        _input.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => Move(Vector2.zero);
        
        _input.Player.Shoot.performed += ctx => StartAttack(shotChargeTime);
        _input.Player.Shoot.canceled += ctx => EndAttack(_shootAttacks, SHOOT_ANIM_TRIGGER, shotChargeTime);
        _input.Player.Punch.canceled += ctx => EndAttack(_punchAttacks, PUNCH_ANIM_TRIGGER);
        _input.Player.BossSkill.canceled += ctx => EndAttack(_bossAttacks, BOSS_ANIM_TRIGGER);

        _input.Dialogue.NextDialogue.performed += ctx => NextDialogue();
        _input.Dialogue.SkipDialogue.performed += ctx => SkipDialogue();
        _input.Dialogue.SpeedUpDialogue.started += ctx => SpeedUpDialogue(true);
        _input.Dialogue.SpeedUpDialogue.canceled += ctx => SpeedUpDialogue(false);

        _input.Universal.Pause.performed += ctx => ToggleMenu();
        
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueStart += OnDialogueStart;
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd += OnDialogueEnd;
    }

    private void ToggleMenu()
    {
        // No opening menu during dialogue (which also means no closing)
        // but since we can never open it with dialogue, that shouldn't be needed
        if(ServiceLocator.Instance.Get<DialogueSystem>().IsDialogueActive())
        {
            return;
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
        _animator.SetBool(CHARGE_ANIM_BOOL, true);
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
        _animator.SetBool(CHARGE_ANIM_BOOL, false);
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
    
    private void OnEnable()
    {
        _input.Enable();
        _input.Dialogue.Disable();

    }

    private void OnDisable()
    {
        _input.Disable();
    }
}