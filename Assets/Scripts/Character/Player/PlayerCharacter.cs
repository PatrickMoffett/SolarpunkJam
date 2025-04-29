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
    [Tooltip("IFrame")]
    [SerializeField] private float _iFrameDuration = 1f;
    [SerializeField] private Color _iFrameColor = new Color(1f, 1f, 1f, .5f);
    [SerializeField] private float _iFrameColorOscillationFrequency = 5f;

    private bool _isInvulnerable = false;

    private PlayerController _playerController;
    private PlayerMovementComponent _movementComponent;
    private SpriteColorOscillator _spriteColorOscillator;
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
        _spriteColorOscillator.StartSpriteOscillation(
            Color.white,
            _iFrameColor,
            _iFrameDuration,
            _iFrameColorOscillationFrequency);

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

        _spriteColorOscillator = GetComponent<SpriteColorOscillator>();
        Assert.IsNotNull(_spriteColorOscillator, $"SpriteColorOscillator not found on {gameObject.name}.");
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