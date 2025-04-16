using System;
using UnityEngine;

public class Enemy : Character
{
    protected void Start()
    {
        _attributeSet.AttributesDictionary[GlobalAttributes.HealthAttribute].OnValueChanged += OnHealthChanged;
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
        Destroy(gameObject);
    }
}