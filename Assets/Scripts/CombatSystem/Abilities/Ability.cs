using UnityEditor;
using UnityEngine;

public class AbilityTargetData //Expand/inherit from this class as more data is needed
{
    public Vector3 sourceCharacterLocation;
    public Vector3 sourceCharacterDirection;
    public Vector3 targetLocation;
    public GameObject targetGameObject;
}
public abstract class Ability : ScriptableObject
{
    public bool passiveAbility = false;

    protected GameObject _owner;
    protected AttributeSet _attributes;
    protected CombatSystem _combatSystem;

    public StatusEffect activationCost;
    public StatusEffect cooldown;

    private StatusEffectInstance _appliedCooldown;
    
    public void Initialize(GameObject owner)
    {
        _owner = owner;
        _combatSystem = _owner.GetComponent<CombatSystem>();
        _attributes = _owner.GetComponent<AttributeSet>();
    }
    public virtual bool TryActivate(AbilityTargetData activationData)
    {
        if ((_appliedCooldown == null || !_combatSystem.GetStatusEffects().Contains(_appliedCooldown)) 
            &&
            (activationCost == null || _combatSystem.TryActivationCost(activationCost)))
        {
            Activate(activationData);
            
            if (cooldown != null)
            {
                OutgoingStatusEffectInstance effect = new OutgoingStatusEffectInstance(cooldown, _combatSystem);
                _appliedCooldown = _combatSystem.ApplyStatusEffect(effect);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
    protected abstract void Activate(AbilityTargetData activationData);
}
