using UnityEngine;
using UnityEngine.Assertions;

public class Enemy : Character
{
    
    private LootSpawner _lootSpawner;
    private Rigidbody2D _rigidbody2D;
    public void Kill()
    {
        Die();
    }
    protected virtual void Start()
    {
        Attribute health = _attributeSet.GetAttribute(GlobalAttributes.HealthAttribute);
        Assert.IsNotNull(health, $"Health attribute not found in the attribute set.");
        health.OnValueChanged += OnHealthChanged;

        Attribute knockback = _attributeSet.GetAttribute(GlobalAttributes.KnockbackAttribute);
        Assert.IsNotNull(knockback, "Knockback attribute not found in the attribute set.");
        knockback.OnValueChanged += OnApplyKnockback;
        
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _lootSpawner = GetComponent<LootSpawner>();
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
        OnCharacterDeath?.Invoke(this);

        if (_lootSpawner != null)
        {
            _lootSpawner.SpawnLoot();
        }

        Destroy(gameObject);
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