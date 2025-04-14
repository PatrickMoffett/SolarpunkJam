using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Attribute
{
    public Attribute(){ }

    public Attribute(float value)
    {
        baseValue = value;
    }
    /// <summary>
    /// Action Invoked whenever the CurrentValue of this Attribute changes
    /// </summary>
    public event Action<Attribute,float> OnValueChanged;

    /// <summary>
    /// Dictionary of Attribute modifiers
    /// </summary>
    private List<AttributeModifier> _modifiers = new List<AttributeModifier>();
    
    /// <summary>
    /// base value of this attribute before modifiers
    /// </summary>
    [SerializeField]private float baseValue;

    //Attribute representing the max base value of this attribute
    private Attribute _maxBaseAttribute;
    
    /// <summary>
    /// Base Value of the attribute before modifiers
    /// </summary>
    public float BaseValue
    {
        get
        {
            return baseValue;
        }
        set
        {
            float previous = CurrentValue;
            if (_maxBaseAttribute != null)
            {
                baseValue = Mathf.Min(value, _maxBaseAttribute.CurrentValue);
            }
            else
            {
                baseValue = value;
            }

            UpdateCurrentValue();
            //TODO: Find a better way to announce changes made here
            //Event can't be announced here because this might be the health attribute,
            //and we might need to clamp this value
            //but we can't clamp the value here, because the attribute might not be one with a MAX amount
            //CATCH 22
            //OnValueChanged?.Invoke(this,previous);
        }
    }
    
    /// <summary>
    /// Current Value of the attribute after all modifiers
    /// </summary>
    public float CurrentValue { get; private set; }

    public void SetMaxAttribute(Attribute modifiableAttributeValue)
    {
        _maxBaseAttribute = modifiableAttributeValue;
    }
    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Updates the current value of this attribute including all the modifiers affecting it
    /// </summary>
    public void UpdateCurrentValue()
    {
        float previousValue = CurrentValue;
        float sumToAdd = 0f;
        float multiplier = 1f;
        foreach (var modifier in _modifiers)
        {
            switch (modifier.operation)
            {
                case AttributeModifier.Operator.Add:
                    sumToAdd += modifier.attributeModifierValue.Value;
                    break;
                case AttributeModifier.Operator.Subtract:
                    sumToAdd -= modifier.attributeModifierValue.Value;
                    break;
                case AttributeModifier.Operator.Multiply:
                    multiplier *= modifier.attributeModifierValue.Value;
                    break;
                case AttributeModifier.Operator.Divide:
                    multiplier /= modifier.attributeModifierValue.Value;
                    break;
                case AttributeModifier.Operator.Set:
                    CurrentValue = modifier.attributeModifierValue.Value;
                    return;
                default:
                    Debug.LogError("Unexpected Operator in attributeMod");
                    break;
            }
        }
        CurrentValue = (baseValue + sumToAdd) * multiplier;
        OnValueChanged?.Invoke(this,previousValue);
    }
    
    /// <summary>
    /// Add a modifier that will change the value of the attribute
    /// </summary>
    /// <param name="modifier"></param>
    public void AddModifier(AttributeModifier modifier)
    {
        _modifiers.Add(modifier);
        UpdateCurrentValue();
    }
    /// <summary>
    /// Remove a modifier that was changing the value of the attribute
    /// </summary>
    /// <param name="modifier"></param>
    public void RemoveModifier(AttributeModifier modifier)
    {
        _modifiers.Remove(modifier);
        UpdateCurrentValue();
    }

    public void InstantlyApply(AttributeModifier modifier)
    {
        float previous = CurrentValue;
        switch (modifier.operation)
        {
            case AttributeModifier.Operator.Add:
                BaseValue += modifier.attributeModifierValue.Value;
                break;
            case AttributeModifier.Operator.Subtract:
                BaseValue -= modifier.attributeModifierValue.Value;
                break;
            case AttributeModifier.Operator.Multiply:
                BaseValue *= modifier.attributeModifierValue.Value;
                break;
            case AttributeModifier.Operator.Divide:
                BaseValue /= modifier.attributeModifierValue.Value;
                break;
            case AttributeModifier.Operator.Set:
                BaseValue = modifier.attributeModifierValue.Value;
                return;
            default:
                Debug.LogError("Unexpected Operator in attributeMod");
                break;
        }
        UpdateCurrentValue();
    }

    private const float TOLERANCE = 0.001f;
}
