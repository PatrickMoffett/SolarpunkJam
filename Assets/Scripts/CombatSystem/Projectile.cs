using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Projectile : MonoBehaviour
{          
    public bool destroyOnCollision = true;
    public event Action<GameObject> OnHitObject;
    public event Action OnDestroyed;
    private readonly List<OutgoingStatusEffectInstance> _effectsToApply = new List<OutgoingStatusEffectInstance>();

    private static GameObject _bucket;
    private void Start()
    {
        if (!_bucket)
        {
            _bucket = new GameObject("Projectile_Bucket");
        }
        transform.parent = _bucket.transform;
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        CombatSystem combatSystem = col.gameObject.GetComponent<CombatSystem>();
        if (combatSystem)
        {
            foreach (var effect in _effectsToApply)
            {
                combatSystem.ApplyStatusEffect(effect);
            }
        }

        if (destroyOnCollision)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        CombatSystem combatSystem = col.gameObject.GetComponent<CombatSystem>();
        if (combatSystem)
        {
            foreach (var effect in _effectsToApply)
            {
                combatSystem.ApplyStatusEffect(effect);
            }
        }
        if (destroyOnCollision)
        {
            Destroy(gameObject);
        }
    }

    public void AddStatusEffects(List<OutgoingStatusEffectInstance> effects)
    {
        _effectsToApply.AddRange(effects);
    }
}
