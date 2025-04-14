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
    [CreateAssetMenu(menuName="CombatSystem/Abilities/RangedAttackAbility")]
    public class RangedAttackAbility : Ability
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float projectileVelocity = 10f;
        [SerializeField] private List<StatusEffect> effectsToApplyOnHit;
        [SerializeField] private SimpleAudioEvent audioEvent;

        protected override void Activate(AbilityTargetData activationData)
        {
            _owner.GetComponent<Animator>().SetBool("Attacking", true);
            Vector2 direction = activationData.sourceCharacterDirection;
            //set rotation and spawn projectile
            var rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90));
            GameObject projectile = Instantiate(projectilePrefab,_owner.transform.position,rotation);
            Destroy(projectile,projectileLifetime);
            //Instantiate status effects
            List<OutgoingStatusEffectInstance> statusEffectInstances = new List<OutgoingStatusEffectInstance>();
            foreach (var effect in effectsToApplyOnHit)
            {
                statusEffectInstances.Add(new OutgoingStatusEffectInstance(effect,_combatSystem));
            }
            
            //Add Status Effect Instances to projectile
            projectile.GetComponent<Projectile>().AddStatusEffects(statusEffectInstances);
            
            //Set projectile velocity
            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileVelocity;

            if (audioEvent)
            {
                audioEvent.Play(_owner);
            }
        }
    }
}