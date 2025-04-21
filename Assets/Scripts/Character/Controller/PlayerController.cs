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
    
    private InputSystem_Actions _input;
    private PlayerMovementComponent _movementComponent;

    [Header("Attack information")]
    [Tooltip("The various shoot attacks the player currently has access to")]
    [SerializeField] private List<Ability> _shootAttacks;

    [Tooltip("The various punch attacks the player currently has access to")]
    [SerializeField] private List<Ability> _punchAttacks;

    [Tooltip("The various boss skills the player currently has access to")]
    [SerializeField] private List<Ability> _bossAttacks;

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

        _input.Player.Jump.started += ctx => StartJump();
        _input.Player.Jump.canceled += ctx => EndJump();
        _input.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => Move(Vector2.zero);

        _input.Player.Shoot.performed += ctx => StartAttack();
        _input.Player.Shoot.canceled += ctx => EndAttack(_shootAttacks, SHOOT_ANIM_TRIGGER, shotChargeTime);

        _input.Player.Punch.canceled += ctx => EndAttack(_punchAttacks, PUNCH_ANIM_TRIGGER);

        _input.Player.BossSkill.canceled += ctx => EndAttack(_bossAttacks, BOSS_ANIM_TRIGGER);
    }

    protected void OnDestroy()
    {
        if (Services.ServiceLocator.Instance.Get<PlayerManager>().GetPlayerController() == this)
        {
            Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerController(null);
        }
    }
    private void StartJump()
    {
        _movementComponent.SetJump(true);
    }
    private void EndJump()
    {
        _movementComponent.SetJump(false);
    }
    private void Move(Vector2 vector2)
    {
        _movementComponent.SetMoveDirection(vector2);
        if (vector2 != Vector2.zero)
        {
            attackDirection = vector2;
        }
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
    }

    private void OnDisable()
    {
        _input.Disable();
    }
}