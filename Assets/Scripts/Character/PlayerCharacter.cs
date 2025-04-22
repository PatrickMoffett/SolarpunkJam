using Services;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(PlayerController))]
public class PlayerCharacter : Character
{
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
    protected void Start()
    {
        Attribute health = _attributeSet.GetAttribute(GlobalAttributes.HealthAttribute);
        Assert.IsNotNull(health, $"Health attribute not found in the attribute set.");
        health.OnValueChanged += OnHealthChanged;
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