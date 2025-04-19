using System;
using System.Collections;
using System.Collections.Generic;
using Abilities;
using Services;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementComponent))]
public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions _input;
    private PlayerMovementComponent _movementComponent;

    [Header("Attack information")]
    [Tooltip("The various attacks the player currently has access to")]
    [SerializeField] private List<RangedAttackAbility> _attacks;

    [Tooltip("How long thew player needs to hold the button down for a charge attack. Make negative to disable")]
    [SerializeField] private float chargeTime;

    private bool isCharging = false;
    private float chargeTimeCounter = 0;
    private Vector2 attackDirection = Vector2.right;
    private void Awake()
    {
        _input = new InputSystem_Actions();
        _movementComponent = GetComponent<PlayerMovementComponent>();

        _input.Player.Jump.started += ctx => StartJump();
        _input.Player.Jump.canceled += ctx => EndJump();
        _input.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => Move(Vector2.zero);

        _input.Player.Attack.performed += ctx => StartAttack();
        _input.Player.Attack.canceled += ctx => EndAttack();
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

    private void EndAttack()
    {
        AbilityTargetData target = new AbilityTargetData();
        target.sourceCharacterLocation = transform.position;
        target.sourceCharacterDirection = attackDirection;
        isCharging = false;
        RangedAttackAbility chosenAttack = _attacks[0];
        if (chargeTimeCounter >= chargeTime)
        {
            Debug.Log($"[{GetType().Name}] Charged attack firing!");
            chosenAttack = _attacks[1];
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