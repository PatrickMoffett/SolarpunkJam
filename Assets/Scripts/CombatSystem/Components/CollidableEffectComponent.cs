using UnityEngine;
using System.Collections.Generic;
public class CollidableEffectComponent : MonoBehaviour
{
    [SerializeField] private bool _applyOnCollision = true;
    [SerializeField] private bool _applyOnTrigger = true;
    [SerializeField] private bool _destroyOnApplication = true;
    [SerializeField] private List<StatusEffect> _effectsToApply = new List<StatusEffect>();

    private void TryToApplyToGameObject(GameObject obj)
    {
        CombatSystem combatSystem = obj.gameObject.GetComponent<CombatSystem>();
        if (combatSystem == null)
        {
            return;
        }

        foreach (var effect in _effectsToApply)
        {
            OutgoingStatusEffectInstance effectInstance = new OutgoingStatusEffectInstance(effect, combatSystem);
            combatSystem?.ApplyStatusEffect(effectInstance);
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