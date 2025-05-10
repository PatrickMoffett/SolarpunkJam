using Services;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovementComponent))]
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(SpriteColorOscillator))]
public class PlayerCharacter : Character
{
    [Tooltip("IFrames")]
    [SerializeField] private float _iFrameDuration = 1f;
    [SerializeField] private Color _iFrameColor = new Color(1f, 1f, 1f, .5f);
    [SerializeField] private float _iFrameColorOscillationFrequency = 5f;
    [SerializeField] private StatusEffect _immunityEffect;

    private PlayerController _playerController;
    private PlayerMovementComponent _movementComponent;
    private SpriteColorOscillator _spriteColorOscillator;
    protected override void Awake()
    {
        base.Awake();
        Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerCharacter(this);
    }
    protected void OnEnable()
    {
        _combatSystem.OnStatusEffectAdded += OnStatusEffectAdded;
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

        _spriteColorOscillator = GetComponent<SpriteColorOscillator>();
        Assert.IsNotNull(_spriteColorOscillator, $"SpriteColorOscillator not found on {gameObject.name}.");

        Assert.IsNotNull(_immunityEffect, $"Immunity effect not assigned in {gameObject.name}.");
    }
    protected void OnDisable()
    {
        _combatSystem.OnStatusEffectAdded -= OnStatusEffectAdded;
    }
    protected void OnDestroy()
    {
        if (Services.ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter() == this)
        {
            Services.ServiceLocator.Instance.Get<PlayerManager>().SetPlayerCharacter(null);
        }
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
        OutgoingStatusEffectInstance immunityEffectOutgoing = new OutgoingStatusEffectInstance(_immunityEffect, _combatSystem);
        var immunityEffectInstance = _combatSystem.ApplyStatusEffect(immunityEffectOutgoing);

        _spriteColorOscillator.StartSpriteOscillation(
            Color.white,
            _iFrameColor,
            immunityEffectInstance.Duration.Value,
            _iFrameColorOscillationFrequency);

        // apply knockback
        Vector2 knockbackDirection = (transform.position - instance.GetSourceCombatSystem().gameObject.transform.position).normalized;
        _movementComponent.ApplyKnockback(knockbackDirection);
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
        OnCharacterDeathStart?.Invoke(this);
        ServiceLocator.Instance.Get<ApplicationStateManager>().PushState<GameOverState>(true);
    }
}