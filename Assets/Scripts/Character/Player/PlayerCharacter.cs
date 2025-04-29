using Services;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovementComponent))]
[RequireComponent(typeof(CombatSystem))]


public class PlayerCharacter : Character
{ 
    private PlayerController _playerController;
    private PlayerMovementComponent _movementComponent;

    protected override void Awake()
    {
        base.Awake();
        Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerCharacter(this);
    }
    protected void OnDestroy()
    {
        if (Services.ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter() == this)
        {
            Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerCharacter(null);
        }
    }
    protected void OnEnable()
    {
        _combatSystem.OnStatusEffectAdded += OnStatusEffectAdded;
    }
    protected void OnDisable()
    {
        _combatSystem.OnStatusEffectAdded -= OnStatusEffectAdded;
    }

    private void OnStatusEffectAdded(StatusEffectInstance instance)
    {
        foreach(var modifier in instance.AttributeModifiers)
        {
            if (modifier.attribute == GlobalAttributes.HealthAttribute)
            {
                bool isNegative = 
                    (modifier.operation == AttributeModifier.Operator.Subtract
                    && modifier.attributeModifierValue.Value > 0f)
                    || (modifier.operation == AttributeModifier.Operator.Add
                    && modifier.attributeModifierValue.Value < 0f) 
                    ? true : false;
                if(isNegative)
                {
                    // apply knockback
                    OnTakeDamage(instance);
                }
            }
        }
    }

    private void OnTakeDamage(StatusEffectInstance instance)
    {
        // apply knockback
        Vector2 knockbackDirection = (transform.position - instance.GetSourceCombatSystem().gameObject.transform.position).normalized;
        _movementComponent.ApplyKnockback(knockbackDirection);
    }

    protected void Start()
    {
        Attribute health = _attributeSet.GetAttribute(GlobalAttributes.HealthAttribute);
        Assert.IsNotNull(health, $"Health attribute not found in the attribute set.");
        health.OnValueChanged += OnHealthChanged;

        _movementComponent = GetComponent<PlayerMovementComponent>();
        Assert.IsNotNull(_movementComponent, $"PlayerMovementComponent not found on {gameObject.name}.");

        _playerController = GetComponent<PlayerController>();
        Assert.IsNotNull(_playerController, $"PlayerController not found on {gameObject.name}.");
    }

    private void OnHealthChanged(Attribute attribute, float newValue)
    {
        if (attribute.CurrentValue <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnCharacterDeath?.Invoke(this);
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameOverState>();
    }
}