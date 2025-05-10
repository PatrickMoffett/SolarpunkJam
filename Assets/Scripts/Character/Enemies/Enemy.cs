using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LootSpawner))]
[RequireComponent(typeof(CombatSystem))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
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
    [SerializeField] private List<GameObject> _otherObjectsToSpawnOnDeath = new List<GameObject>();
    [SerializeField] private Vector2Int _cleanseRange;
    [SerializeField] private float _deathDestroyDelay = 0.5f;
    
    private const string ANIM_DYING = "Dying";
    private const float UPWARD_KNOCKBACK = 1f;
    protected bool _isDying = false;
    
    private LootSpawner _lootSpawner;
    protected Rigidbody2D _rigidbody2D;
    protected Animator _animator;
    public void Kill()
    {
        if (!_isDying)
        {
            Die();
        }
    }
    protected override void Awake()
    {
        base.Awake();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rigidbody2D, $"Rigidbody2D not found on {gameObject.name}.");

        _lootSpawner = GetComponent<LootSpawner>();
        Assert.IsNotNull(_lootSpawner, $"LootSpawner not found on {gameObject.name}.");

        _animator = GetComponent<Animator>();
        Assert.IsNotNull(_animator, "Animator not found on BossAI.");
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
        if (_applyCollisionEffects && !_isDying)
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
        if(_isDying)
        {
            return; // already dying
        }

        _isDying = true;
        _animator.SetBool(ANIM_DYING, true);

        OnCharacterDeathStart?.Invoke(this);

        StartCoroutine(DeathDelay());
    }
    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(_deathDestroyDelay);
        FinishDeath();
    }
    protected virtual void FinishDeath()
    {
        Destroy(gameObject);
        if (_lootSpawner != null)
        {
            _lootSpawner.SpawnLoot();
        }
        if (_cleansedObjectToSpawn != null)
        {
            Instantiate(_cleansedObjectToSpawn, transform.position, Quaternion.identity);
        }
        if (_tileCleanser != null)
        {
            GameObject cleanser = Instantiate(_tileCleanser, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Quaternion.identity);
            TileCleanse tileCleanser = cleanser.GetComponent<TileCleanse>();
            Assert.IsNotNull(tileCleanser, $"TileCleanse component not found on {gameObject.name}.");
            tileCleanser.tileCleanse(_cleanseRange);
        }
        foreach(var prefab in _otherObjectsToSpawnOnDeath)
        {
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }
    }
    private void OnApplyKnockback(Attribute attribute, float previousValue)
    {
        if (_isDying)
        {
            return; // already dying
        }
        // Don't let a 0 set through
        if (attribute.CurrentValue == 0)
        {
            return;
        }
        // Set the knockback
        _rigidbody2D.linearVelocityX = attribute.CurrentValue;
        _rigidbody2D.linearVelocityY = UPWARD_KNOCKBACK;
        attribute.SetAttributeBaseValueDangerous(0);
    }
    
    
}