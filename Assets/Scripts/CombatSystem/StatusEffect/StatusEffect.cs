using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="CombatSystem/StatusEffect")]
public class StatusEffect : ScriptableObject
{
    /// <summary>
    /// Instant Effects apply instantly and permanently. They cannot be removed.
    /// Example use: Damage, Healing, Permanent Stat increase, Leveling Up
    ///
    /// Periodic Effects are effects that periodically apply an Instant Effect
    /// They will periodically apply their effect until removed or duration time expires
    /// Example use: Poison, health regen
    /// 
    /// Duration Effects Last for the duration amount, or until manually removed.
    /// Effects are reversable
    /// Example use: Rage, Temporary Speed boost
    ///
    /// Infinite Effects last until manually removed.
    /// Effects are reversable
    /// Example use: Equipment
    /// </summary>
    public enum DurationType
    {
        Instant, // Applies instantly, and permanently
        Duration, // Lasts for Duration amount or until removed manually
        Infinite // Lasts until removed manually
    }
    /// <summary>
    /// The duration type of the effect
    /// </summary>
    public DurationType durationType = DurationType.Instant;
    /// <summary>
    /// The Duration of the effect (Only applies to effects with DurationType = Duration)
    /// </summary>
    [ShowIf(nameof(DurationTypeEqualsDuration))]
    public AttributeModifierValue duration;
    /// <summary>
    /// Bool indicating if the effect happens at set intervals
    /// </summary>
    [ShowIf(ActionOnConditionFail.DontDraw, ConditionOperator.Or, 
        nameof(DurationTypeEqualsDuration), nameof(DurationTypeEqualsInfinite))]
    public bool isPeriodic = false;
    /// <summary>
    /// How often to reapply the effect
    /// </summary>
    [ShowIf(nameof(isPeriodic))]
    public float periodicRate = 0f;
    /// <summary>
    /// List of attribute modifiers this effect applies
    /// </summary>
    public List<AttributeModifier> attributeModifiers = new List<AttributeModifier>();
    /// <summary>
    /// List of abilities granted while this effect is active
    /// </summary>
    public List<Ability> grantedAbilities = new List<Ability>();

    public bool DurationTypeEqualsDuration()
    {
        return durationType == DurationType.Duration;
    }

    public bool DurationTypeEqualsInfinite()
    {
        return durationType == DurationType.Infinite;
    }
}
