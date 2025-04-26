using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Services;


[RequireComponent(typeof(CombatSystem))]
public class CollidableEffectComponent : MonoBehaviour
{
    [SerializeField] private bool _applyOnCollision = true;
    [SerializeField] private bool _applyOnTrigger = true;
    [SerializeField] private bool _destroyOnApplication = true;
    [SerializeField] private bool _canApplyToNonPlayer = false;
    [SerializeField] private List<StatusEffect> _effectsToApply = new List<StatusEffect>();

    private CombatSystem _combatSystem;

    private void Awake()
    {
        _combatSystem = GetComponent<CombatSystem>();
        Assert.IsNotNull(_combatSystem, "CombatSystem component is missing on the GameObject.");
    }

    private void TryToApplyToGameObject(GameObject obj)
    {
        CombatSystem targetCombatSystem = obj.gameObject.GetComponent<CombatSystem>();
        if (targetCombatSystem == null)
        {
            return;
        }

        if(!_canApplyToNonPlayer)
        {
            PlayerCharacter pc = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter();
            if(pc.gameObject != targetCombatSystem.gameObject)
            {
                return;
            }
        }

        foreach (var effect in _effectsToApply)
        {
            OutgoingStatusEffectInstance effectInstance = new OutgoingStatusEffectInstance(effect, _combatSystem);
            targetCombatSystem?.ApplyStatusEffect(effectInstance);
        }
        if (_destroyOnApplication)
        {
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        TryToApplyToGameObject(col.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        TryToApplyToGameObject(col.gameObject);
    }
}