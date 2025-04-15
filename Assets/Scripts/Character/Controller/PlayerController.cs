using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementComponent))]
public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions _input;
    private PlayerMovementComponent _movementComponent;
    private void Awake()
    {
        _input = new InputSystem_Actions();
        _movementComponent = GetComponent<PlayerMovementComponent>();

        _input.Player.Jump.started += ctx => StartJump();
        _input.Player.Jump.canceled += ctx => EndJump();
        _input.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        _input.Player.Move.canceled += ctx => Move(Vector2.zero);
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

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }
}