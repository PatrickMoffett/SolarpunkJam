using System;
using UnityEngine;

[Serializable]
public class AttributeModifierValue
{
    public enum ValueType
    {
        Constant,
        AttributeBased,
        CustomCalculation
    }

    public enum AttributeSetToUse
    {
        Source,
        Target
    }
    
    public bool ValueTypeEqualsConstant()
    {
        return valueType == ValueType.Constant;
    }

    public bool ValueTypeEqualsAttributeBased()
    {
        return valueType == ValueType.AttributeBased;
    }

    public bool ValueTypeEqualsCustomCalc()
    {
        return valueType == ValueType.CustomCalculation;
    }
    
    [SerializeField]private ValueType valueType;
    
    [ShowIf(nameof(ValueTypeEqualsConstant))]
    [SerializeField]
    private float constantFloat;
    
    [ShowIf(nameof(ValueTypeEqualsAttributeBased))]
    [SerializeField]
    private AttributeSetToUse attributeSet;

    [ShowIf(nameof(ValueTypeEqualsAttributeBased))]
    [SerializeField]
    private AttributeType sourceAttributeType;

    [ShowIf( nameof(ValueTypeEqualsAttributeBased))]
    [SerializeField]
    private float preCoefficientAddition;

    [ShowIf( nameof(ValueTypeEqualsAttributeBased))]
    [SerializeField]
    private float coefficient;
    
    [ShowIf( nameof(ValueTypeEqualsAttributeBased))]
    [SerializeField]
    private float postCoefficientAddition;

    [ShowIf(nameof(ValueTypeEqualsCustomCalc))]
    [SerializeField]
    private CustomValueCalculation customValueCalculation;

    private CombatSystem _sourceCombatSystem;
    private CombatSystem _targetCombatSystem;

    public float Value
    {
        get
        {
            if (valueType == ValueType.AttributeBased
                && attributeSet == AttributeSetToUse.Source
                && !_sourceCombatSystem)
            {
                //TODO: Fix bug where source target has died and this prevents applying their effects
                //try snapshotting their attribute set
                Debug.LogError("No Source Combat System Found (May have been destroyed");
                return 0f;
            }
            if (!( _targetCombatSystem))
            {
                Debug.LogError("Attribute Modifier Value missing properties");
                return 0f;
            }
            switch (valueType)
            {
                case ValueType.Constant:
                    return constantFloat;
                case ValueType.AttributeBased:
                    if (attributeSet == AttributeSetToUse.Source)
                    {
                        return ((_sourceCombatSystem.GetAttributeSet().GetCurrentAttributeValue(sourceAttributeType)
                                 + preCoefficientAddition) * coefficient) + postCoefficientAddition;
                    }
                    else
                    {
                        return ((_targetCombatSystem.GetAttributeSet().GetCurrentAttributeValue(sourceAttributeType)
                                 + preCoefficientAddition) * coefficient) + postCoefficientAddition;
                    }
                case ValueType.CustomCalculation:
                    if (!customValueCalculation)
                    {
                        Debug.LogError("Custom Calculation Not Set");
                    }
                    return customValueCalculation.Calculate(_sourceCombatSystem, _targetCombatSystem);
                default:
                    Debug.LogError("Unsupported Value Type used");
                    return 0f;
            }
        }
    }

    public void SetSourceCombatSystem(CombatSystem combatSystem)
    {
        _sourceCombatSystem = combatSystem;
    }
    public void SetTargetCombatSystem(CombatSystem combatSystem)
    {
        _targetCombatSystem = combatSystem;
    }
}
