using UnityEngine;
using System.Collections.Generic;
public class CollidableEffectComponent : MonoBehaviour
{
    [SerializeField] private bool ApplyOnCollision = true;
    [SerializeField] private bool ApplyOnTrigger = true;
    [SerializeField] private bool destroyOnApplication = true;
    [SerializeField] private List<StatusEffect> EffectsToApply = new List<StatusEffect>();

    private void TryToApplyToGameObject(GameObject obj)
    {
        CombatSystem combatSystem = obj.gameObject.GetComponent<CombatSystem>();
        if (combatSystem == null)
        {
            return;
        }

        foreach (var effect in EffectsToApply)
        {
            OutgoingStatusEffectInstance effectInstance = new OutgoingStatusEffectInstance(effect, combatSystem);
            combatSystem?.ApplyStatusEffect(effectInstance);
        }
        if (destroyOnApplication)
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