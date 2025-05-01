using UnityEngine.Assertions;

public class Enemy : Character
{
    private LootSpawner _lootSpawner;
    public void Kill()
    {
        Die();
    }
    protected virtual void Start()
    {
        Attribute health = _attributeSet.GetAttribute(GlobalAttributes.HealthAttribute);
        Assert.IsNotNull(health, $"Health attribute not found in the attribute set.");
        health.OnValueChanged += OnHealthChanged;

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
}