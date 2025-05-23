using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AttributeSet))]
[RequireComponent(typeof(CombatTagContainer))]
public class CombatSystem : MonoBehaviour
{
    [SerializeField]
    private List<StatusEffect> startingEffects = new List<StatusEffect>();
    
    private AttributeSet _attributeSet;
    private CombatTagContainer _combatTagContainer;

    private readonly List<StatusEffectInstance> _currentStatusEffects = new List<StatusEffectInstance>();

    public event Action<StatusEffectInstance> OnStatusEffectAdded;
    public event Action<StatusEffectInstance> OnStatusEffectRemoved;

    // Start is called before the first frame update
    private void Start()
    {
        _attributeSet = GetComponent<AttributeSet>();
        Assert.IsNotNull(_attributeSet, "AttributeSet is not assigned in CombatSystem.");

        _combatTagContainer = GetComponent<CombatTagContainer>();
        Assert.IsNotNull(_combatTagContainer, "CombatTagContainer is not assigned in CombatSystem.");
        //force update current values before apply effects
        _attributeSet.UpdateCurrentValues();
        
        //apply starting effects
        foreach (var effect in startingEffects)
        {
            StatusEffectInstance statusEffectToApply = 
                new StatusEffectInstance(new OutgoingStatusEffectInstance(effect, this),this);
            ApplyStatusEffectInstance(statusEffectToApply);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public StatusEffectInstance ApplyStatusEffect(OutgoingStatusEffectInstance effect)
    {
        //set this as the targetCombatSystem
        StatusEffectInstance effectToApply = new StatusEffectInstance(effect, this);

        return ApplyStatusEffectInstance(effectToApply);
    }
    
    private StatusEffectInstance ApplyStatusEffectInstance(StatusEffectInstance effectToApply)
    {
        if (_combatTagContainer.HasAnyTag(effectToApply._effect._effect.immunityTags)) // TODO: WTF
        {
            //if the target has any of the tags to ignore, don't apply the effect
            return null;
        }

        if (effectToApply.DurationType == StatusEffect.DurationType.Instant)
        {
            //apply all modifiers instantly
            foreach (var modifier in effectToApply.AttributeModifiers)
            {
                _attributeSet.ApplyInstantModifier(modifier);
            }
        }
        else
        {
            _combatTagContainer.AddTags(effectToApply._effect._effect.providedTags); // TODO: WTF

            //if the effect has a duration start a coroutine to remove the effect when it's done.
            if (effectToApply.DurationType == StatusEffect.DurationType.Duration)
            {
                StartCoroutine(WaitToRemoveStatusEffect(effectToApply));
            }

            //Add it to the list of current status effects
            _currentStatusEffects.Add(effectToApply);

            //if the effect happens periodically, start a coroutine to do that
            if (effectToApply.IsPeriodic)
            {
                StartCoroutine(ApplyPeriodicEffect(effectToApply));
            }
            else
            {
                //otherwise add a modifier to each affected attribute
                foreach (var modifier in effectToApply.AttributeModifiers)
                {
                    _attributeSet.ApplyModifier(modifier);
                }
            }
        }
        OnStatusEffectAdded?.Invoke(effectToApply);
        return effectToApply;
    }
    private IEnumerator ApplyPeriodicEffect(StatusEffectInstance effectToApply)
    {
        //While we have this effect
        while (_currentStatusEffects.Contains(effectToApply)){
            //Apply Modifiers instantly
            foreach (var modifier in effectToApply.AttributeModifiers)
            {
                _attributeSet.ApplyInstantModifier(modifier);
            }
            //and wait periodic rate before doing it again
            yield return new WaitForSeconds(effectToApply.PeriodicRate);
        }
    }

    public void RemoveStatusEffect(StatusEffectInstance effectToRemove)
    {
        _currentStatusEffects.Remove(effectToRemove);
        _combatTagContainer.RemoveTags(effectToRemove._effect._effect.providedTags); // TODO: WTF?
        if (effectToRemove.DurationType == StatusEffect.DurationType.Instant)
        {
            Debug.LogError("Tried to remove Instant Status Effect");
            return;
        }

        if (!effectToRemove.IsPeriodic)
        {
            foreach (var modifier in effectToRemove.AttributeModifiers)
            {
                _attributeSet.RemoveModifier(modifier);
            }
        }

        OnStatusEffectRemoved?.Invoke(effectToRemove);
    }

    public List<StatusEffectInstance> GetStatusEffects()
    {
        return _currentStatusEffects;
    }
    private IEnumerator WaitToRemoveStatusEffect(StatusEffectInstance effectToRemove)
    {
        yield return new WaitForSeconds(effectToRemove.Duration.Value);
        RemoveStatusEffect(effectToRemove);
    }

    public AttributeSet GetAttributeSet()
    {
        return _attributeSet;
    }

    public bool CheckActivationCosts(StatusEffect activationCost)
    {
        //for every modifier
        foreach (var modifier in activationCost.attributeModifiers)
        {
#if UNITY_EDITOR
            //we're pretty much assuming subtract only at this point
            if (modifier.operation != AttributeModifier.Operator.Subtract)
            {
                Debug.LogWarning("Using a modifier other than subtract as an Activation Cost.");
            }
#endif
            //if we subtract and the value is below 0
            if (modifier.operation == AttributeModifier.Operator.Subtract &&
                _attributeSet.GetCurrentAttributeValue(modifier.attribute) < modifier.attributeModifierValue.Value)
            {
                //don't apply the cost and return false
                return false;
            }
        }
        return true;
    }
    public bool TryActivationCost(StatusEffect activationCost)
    {
        if (CheckActivationCosts(activationCost))
        {
            //apply the cost and return true
            StatusEffectInstance statusEffectToApply =
                new StatusEffectInstance(new OutgoingStatusEffectInstance(activationCost, this), this);
            ApplyStatusEffectInstance(statusEffectToApply);
            return true;
        }
        else
        {
            //if we can't apply the cost, return false
            return false;

        }
    }
}
