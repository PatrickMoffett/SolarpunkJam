using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.Serialization;

public class Projectile : MonoBehaviour
{          
    public bool destroyOnCollision = true;
    public event Action<GameObject> OnHitObject;
    public event Action OnDestroyed;
    private readonly List<OutgoingStatusEffectInstance> _effectsToApply = new List<OutgoingStatusEffectInstance>();

    private GameObject owner;
    
    private static GameObject _bucket;

    public bool isBucketable = true;
    private void Start()
    {
        if (!_bucket)
        {
            _bucket = new GameObject("Projectile_Bucket");
        }

        if (isBucketable)
        {
            transform.parent = _bucket.transform;
        }
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.Equals(owner))
        {
            Debug.Log($"[{GetType().Name}] Projectile {gameObject} not interacting with owner {owner}");
            return;
        }
        
        OnHitObject?.Invoke(col.gameObject);

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
            DestroyProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.Equals(owner))
        {
            Debug.Log($"[{GetType().Name}] Projectile {gameObject} not interacting with owner {owner}");
            return;
        }
        
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
            DestroyProjectile();
        }
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
    
    public void AddStatusEffects(List<OutgoingStatusEffectInstance> effects)
    {
        _effectsToApply.AddRange(effects);
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
}
