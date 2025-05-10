using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LootSpawner))]
[RequireComponent(typeof(CombatSystem))]
public class Enemy : Character
{
    [Header("Collision")]
    [SerializeField] private bool dieOnCollisionWithPlayer = false;
    [SerializeField] private bool _applyCollisionEffects = true;
    [ShowIf("_applyCollisionEffects")]
    [SerializeField] private CollisionObserver2D _playerCollisionObserver;
    [ShowIf("_applyCollisionEffects")]
    [SerializeField] private List<StatusEffect> _collisionEffectsToApply;

    [Header("Death")]
    [SerializeField] private GameObject _cleansedObjectToSpawn;
    [SerializeField] private GameObject _tileCleanser;
    [SerializeField] private Vector2Int _cleanseRange;

    private LootSpawner _lootSpawner;
    protected Rigidbody2D _rigidbody2D;
    public void Kill()
    {
        Die();
    }
    protected override void Awake()
    {
        base.Awake();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rigidbody2D, $"Rigidbody2D not found on {gameObject.name}.");

        _lootSpawner = GetComponent<LootSpawner>();
        Assert.IsNotNull(_lootSpawner, $"LootSpawner not found on {gameObject.name}.");
    }
    protected virtual void Start()
    {
        Attribute health = _attributeSet.GetAttribute(GlobalAttributes.HealthAttribute);
        Assert.IsNotNull(health, $"Health attribute not found in the attribute set.");
        health.OnValueChanged += OnHealthChanged;

        Attribute knockback = _attributeSet.GetAttribute(GlobalAttributes.KnockbackAttribute);
        Assert.IsNotNull(knockback, "Knockback attribute not found in the attribute set.");
        knockback.OnValueChanged += OnApplyKnockback;
    }
    protected virtual void OnEnable()
    {
        if (_playerCollisionObserver != null)
        {
            _playerCollisionObserver.OnTriggerEnter += OnPlayerTriggerEnter;
        }
    }
    protected virtual void OnDisable()
    {
        if (_playerCollisionObserver != null)
        {
            _playerCollisionObserver.OnTriggerEnter -= OnPlayerTriggerEnter;
        }
        Attribute health = _attributeSet.GetAttribute(GlobalAttributes.HealthAttribute);
        if (health != null)
        {
            health.OnValueChanged -= OnHealthChanged;
        }
        Attribute knockback = _attributeSet.GetAttribute(GlobalAttributes.KnockbackAttribute);
        if (knockback != null)
        {
            knockback.OnValueChanged -= OnApplyKnockback;
        }
    }
    protected virtual void OnPlayerTriggerEnter(Collider2D collision)
    {
        if (_applyCollisionEffects)
        {
            collision.gameObject.TryGetComponent(out CombatSystem playerCombatSystem);
            if (playerCombatSystem == null)
            {
                return;
            }
            foreach (var effect in _collisionEffectsToApply)
            {
                if (effect != null)
                {
                    playerCombatSystem.ApplyStatusEffect(new OutgoingStatusEffectInstance(effect,_combatSystem));
                }
            }
        }
        if (dieOnCollisionWithPlayer)
        {
            Die();
        }
    }

    private void OnHealthChanged(Attribute attribute, float newValue)
    {
        if (attribute.CurrentValue <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
        OnCharacterDeath?.Invoke(this);
        if (_lootSpawner != null)
        {
            _lootSpawner.SpawnLoot();
        }
        if(_cleansedObjectToSpawn != null)
        {
            Instantiate(_cleansedObjectToSpawn, transform.position, Quaternion.identity);
        }
        if(_tileCleanser != null)
        {
            GameObject cleanser = Instantiate(_tileCleanser, new Vector3 (transform.position.x, transform.position.y-1, transform.position.z), Quaternion.identity);
            TileCleanse tileCleanser = cleanser.GetComponent<TileCleanse>();
            Assert.IsNotNull(tileCleanser, $"TileCleanse component not found on {gameObject.name}.");
            tileCleanser.tileCleanse(_cleanseRange);
        }
    }

    private void OnApplyKnockback(Attribute attribute, float previousValue)
    {
        // Don't let a 0 set through
        if (attribute.CurrentValue == 0)
        {
            return;
        }
        // Set the knockback
        _rigidbody2D.linearVelocityX = attribute.CurrentValue;
        attribute.SetAttributeBaseValueDangerous(0);
    }
    
    
}