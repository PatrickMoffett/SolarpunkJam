using System;
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
    private void Awake()
    {
        _input = new InputSystem_Actions();
        _movementComponent = GetComponent<PlayerMovementComponent>();

        _input.Player.Jump.started += ctx => StartJump();
        _input.Player.Jump.canceled += ctx => EndJump();
        _input.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => Move(Vector2.zero);

        _input.Player.Attack.performed += ctx => Attack();
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
    }

    // TODO: Actually differentiate attacks based on button pressed/time pressed/etc...
    private void Attack()
    {
        //AssetService asr = ServiceLocator.Instance.Get<AssetService>();
        AbilityTargetData target = new AbilityTargetData();
        target.sourceCharacterLocation = transform.position;
        target.sourceCharacterDirection = Vector3.right;
        _attacks[0].Initialize(gameObject);
        _attacks[0].TryActivate(target);
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