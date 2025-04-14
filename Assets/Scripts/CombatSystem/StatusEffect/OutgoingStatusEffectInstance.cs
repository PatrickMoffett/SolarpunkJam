using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class OutgoingStatusEffectInstance
{
    public readonly StatusEffect _effect;
    public readonly CombatSystem _sourceCombatSystem;
    public OutgoingStatusEffectInstance(StatusEffect effect,CombatSystem combatSystem)
    {
        _effect = Object.Instantiate(effect);
        _sourceCombatSystem = combatSystem;
    }

    public OutgoingStatusEffectInstance Clone()
    {
        return new OutgoingStatusEffectInstance(_effect, _sourceCombatSystem);
    }

    public StatusEffect.DurationType DurationType
    {
        get => _effect.durationType;
        set => _effect.durationType = value;
    }
    /// <summary>
    /// The Duration of the effect (Only applies to effects with DurationType = Duration)
    /// </summary>
    public AttributeModifierValue Duration 
    {
        get => _effect.duration;
        set => _effect.duration = value;
    }
    /// <summary>
    /// Bool indicating if the effect happens at set intervals
    /// </summary>
    public bool IsPeriodic
    {
        get => _effect.isPeriodic;
        set => _effect.isPeriodic = value;
    }
    /// <summary>
    /// How often to reapply the effect
    /// </summary>
    public float PeriodicRate    
    {
        get => _effect.periodicRate;
        set => _effect.periodicRate = value;
    }

    /// <summary>
    /// List of attribute modifiers this effect applies
    /// </summary>
    public List<AttributeModifier> AttributeModifiers
    {
        get => _effect.attributeModifiers;
        set => _effect.attributeModifiers = value;
    }

    /// <summary>
    /// List of abilities granted while this effect is active
    /// </summary>
    public List<Ability> GrantedAbilities
    {
        get => _effect.grantedAbilities;
        set => _effect.grantedAbilities = value;
    }

    public string EffectName
    {
        get => _effect.name;
        set => _effect.name = value;
    }

    public string SourceName => _sourceCombatSystem.gameObject.name;
}