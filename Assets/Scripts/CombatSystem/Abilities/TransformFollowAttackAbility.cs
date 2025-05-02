using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Abilities
{
    [CreateAssetMenu(menuName="CombatSystem/Abilities/TransformFollowAttackAbility")]
    public class TransformFollowAttackAbility : Ability
    {
        [SerializeField] private GameEvent stopEvent;
        [SerializeField] private float spawnDelay = 0;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;
        [SerializeField] private List<StatusEffect> effectsToApplyOnHit;
        [SerializeField] private SimpleAudioEvent audioEvent;
        
        protected override void Activate(AbilityTargetData activationData)
        {
            _owner.GetComponent<Animator>().SetBool(activationData.animationTrigger, true);
            ServiceLocator.Instance.Get<MonoBehaviorService>().StartCoroutine(Spawn(activationData));
        }

        private IEnumerator Spawn(AbilityTargetData activationData)
        {
            yield return new WaitForSeconds(spawnDelay);
            Vector2 direction = activationData.sourceCharacterDirection;
            //set rotation and spawn projectile
            var rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90));
            GameObject projectile = Instantiate(projectilePrefab,_owner.transform.position + (Vector3)(spawnOffset * activationData.sourceCharacterDirection),rotation, _owner.transform);
            Projectile projectileComponent = projectile.GetComponent<Projectile>();
            stopEvent.RegisterListener(projectile.GetComponent<GameEventListener>());
            projectileComponent.isBucketable = false;
            projectileComponent.SetOwner(_owner);
            // Set negative lifetime to make it live forever (or until destroyed by something else)
            if (projectileLifetime >= 0)
            {
                Destroy(projectile, projectileLifetime);
            }
            //Instantiate status effects
            List<OutgoingStatusEffectInstance> statusEffectInstances = new List<OutgoingStatusEffectInstance>();
            foreach (var effect in effectsToApplyOnHit)
            {
                statusEffectInstances.Add(new OutgoingStatusEffectInstance(effect,_combatSystem));
            }
            
            //Add Status Effect Instances to projectile
            projectile.GetComponent<Projectile>().AddStatusEffects(statusEffectInstances);
            
            if (audioEvent)
            {
                audioEvent.Play(_owner);
            }
        }
    }
}