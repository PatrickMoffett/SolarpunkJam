
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectInstance
{
    private readonly OutgoingStatusEffectInstance _effect;
    private readonly CombatSystem _targetCombatSystem;
    
    public StatusEffectInstance(OutgoingStatusEffectInstance effect,CombatSystem targetCombatSystem) 
    {
        _effect = effect.Clone();
        _targetCombatSystem = targetCombatSystem;
        InitializeAttributeModifiers();
    }

    private void InitializeAttributeModifiers()
    {
        foreach (AttributeModifier mod in AttributeModifiers)
        {
            mod.attributeModifierValue.SetSourceCombatSystem(_effect._sourceCombatSystem);
            mod.attributeModifierValue.SetTargetCombatSystem(_targetCombatSystem);
        }
        Duration.SetSourceCombatSystem(_effect._sourceCombatSystem);
        Duration.SetTargetCombatSystem(_targetCombatSystem);
    }

    public StatusEffect.DurationType DurationType
    {
        get => _effect.DurationType;
        set => _effect.DurationType = value;
    }
    /// <summary>
    /// The Duration of the effect (Only applies to effects with DurationType = Duration)
    /// </summary>
    public AttributeModifierValue Duration 
    {
        get => _effect.Duration;
        set => _effect.Duration = value;
    }
    /// <summary>
    /// Bool indicating if the effect happens at set intervals
    /// </summary>
    public bool IsPeriodic
    {
        get => _effect.IsPeriodic;
        set => _effect.IsPeriodic = value;
    }
    /// <summary>
    /// How often to reapply the effect
    /// </summary>
    public float PeriodicRate    
    {
        get => _effect.PeriodicRate;
        set => _effect.PeriodicRate = value;
    }

    /// <summary>
    /// List of attribute modifiers this effect applies
    /// </summary>
    public List<AttributeModifier> AttributeModifiers
    {
        get => _effect.AttributeModifiers;
        set => _effect.AttributeModifiers = value;
    }

    /// <summary>
    /// List of abilities granted while this effect is active
    /// </summary>
    public List<Ability> GrantedAbilities
    {
        get => _effect.GrantedAbilities;
        set => _effect.GrantedAbilities = value;
    }

    public string EffectName
    {
        get => _effect.EffectName;
        set => _effect.EffectName = value;
    }

    public string SourceName => _effect.SourceName;
    public string TargetName => _targetCombatSystem.gameObject.name;
}