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

    private bool isCharging = false;
    private float chargeTimeCounter = 0;
    private Vector2 attackDirection = Vector2.right;
    private void Awake()
    {
        Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerController(this);

        _input = new InputSystem_Actions();
        _movementComponent = GetComponent<PlayerMovementComponent>();
        _attributes = GetComponent<AttributeSet>();

        _input.Player.Jump.started += ctx => ExecuteJump();
        _input.Player.Jump.canceled += ctx => AbortJump();
        _input.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => Move(Vector2.zero);
        
        _input.Player.Shoot.performed += ctx => StartAttack();
        _input.Player.Shoot.canceled += ctx => EndAttack(_shootAttacks, SHOOT_ANIM_TRIGGER, shotChargeTime);
        _input.Player.Punch.canceled += ctx => EndAttack(_punchAttacks, PUNCH_ANIM_TRIGGER);
        _input.Player.BossSkill.canceled += ctx => EndAttack(_bossAttacks, BOSS_ANIM_TRIGGER);

        _input.Dialogue.NextDialogue.performed += ctx => NextDialogue();
        _input.Dialogue.SkipDialogue.performed += ctx => SkipDialogue();
        _input.Dialogue.SpeedUpDialogue.started += ctx => SpeedUpDialogue(true);
        _input.Dialogue.SpeedUpDialogue.canceled += ctx => SpeedUpDialogue(false);


        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueStart += OnDialogueStart;
        ServiceLocator.Instance.Get<DialogueSystem>().OnDialogueEnd += OnDialogueEnd;
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
        // Jump slam attack
        if (vector2.y < 0 && !_movementComponent.OnGround)
        {
            attackDirection = Vector2.down;
            _movementComponent.SetMoveDirection(Vector2.down);
            EndAttack(_jumpAttacks, JUMP_SLAM_ANIM_TRIGGER);
        }
    }
    
    private void OnHealthChanged(Attribute attr, float amt)
    {
        // Don't play the damaged animation if the change isn't damage
        if (attr.CurrentValue >= amt)
        {
            return;
        }
        
        // TODO: Better way to do this. Caching the animator seems jank, there has to be anothee location this can be done
        GetComponent<Animator>().SetTrigger(DAMAGED_ANIM_TRIGGER);
    }

    // TODO: Actually differentiate attacks based on button pressed/time pressed/etc...
    private void StartAttack()
    {
        isCharging = true;
        StartCoroutine(IncrementCharge());
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
        chosenAttack.Initialize(gameObject);
        chosenAttack.TryActivate(target);
    }

    private IEnumerator IncrementCharge()
    {
        while (isCharging)
        {
            chargeTimeCounter += Time.deltaTime;
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