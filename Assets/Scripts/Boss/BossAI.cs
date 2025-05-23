﻿using Services;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
public class BossAI : Enemy
{
    [Header("Boss Events")]
    [SerializeField] private GameEvent OnBossStart;
    [SerializeField] private GameEvent OnBossEnd;

    [Header("General")]
    [SerializeField] private float bossSpeed = 5f;
    [Header("Wind Attack")]
    [SerializeField] private float windPushAccel = 10f;
    [SerializeField] private float windAttackDuration = 5f;
    [SerializeField] private float windPushExtension = 2f;
    [SerializeField] private GameObject leftTumbleWeedSpawner;
    [SerializeField] private GameObject rightTumbleWeedSpawner;
    [SerializeField] private GameObject leftManaSpawner;
    [SerializeField] private DoorHandler leftDoorHandler;
    [SerializeField] private DoorHandler rightDoorHandler;
    [SerializeField] private ParticleSystem leftWindParticleSystem;
    [SerializeField] private ParticleSystem rightWindParticleSystem;
    [SerializeField] private Transform leftWindAttackTranform;
    [SerializeField] private Transform rightWindAttackTransform;

    [Header("Charge Attack")]
    [SerializeField] private float chargeAttackMoveSpeed = 10f;
    [SerializeField] private float chargeAttackDelay = .5f;
    [SerializeField] private Transform chargeAttackLocationLeft;
    [SerializeField] private Transform chargeAttackLocationRight;

    [Header("Tornado Spawn Attack")]
    [SerializeField] private int numberOfTornadoes = 5;
    [SerializeField] private float tornadoSpeed = 5f;
    [SerializeField] private float tornadoSpawnInterval = 1f;
    [SerializeField] private GameObject tornadoPrefab;
    [SerializeField] private float tornadoAttackDuration = 10f;
    [SerializeField] private Transform TornadoSpawnAttackLocation;

    [Header("Audio Events")]
    [SerializeField] private AudioEvent _chargeAttackAudioEvent;
    [SerializeField] private AudioEvent _tornadoSummonAttackAudioEvent;

    private Coroutine _bossSequence;
    private Coroutine _delayedWindDisableCoroutine;
    private PlayerMovementComponent _playerMovementComponent;

    private const string ANIM_CHARGE = "Charging";
    private const string ANIM_WIND_ATTACK = "WindAttack";
    private const string ANIM_SUMMONING = "Summoning";

    private Vector3 _cachedScale;
    protected override void OnEnable()
    {
        base.OnEnable();
        _cachedScale = transform.localScale;
        Assert.IsNotNull(OnBossStart, "OnBossStart event not assigned.");
        Assert.IsNotNull(OnBossEnd, "OnBossEnd event not assigned.");
        OnBossStart.OnGameEvent += StartBossFight;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnBossStart.OnGameEvent -= StartBossFight;
    }
    private void SetFacingDirection(bool faceLeft)
    {
        if (faceLeft)
        {
            transform.localScale = new Vector3(_cachedScale.x, _cachedScale.y, _cachedScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-_cachedScale.x, _cachedScale.y, _cachedScale.z);
        }
    }
    protected override void Die()
    {
        EndBossFight();
        StopAllCoroutines();
        base.Die();
    }
    private void StartBossFight()
    {
        _bossSequence = StartCoroutine(PerformBossSequence());
    }
    private void EndBossFight()
    {
        _playerMovementComponent.ResetConstantAcceleration();
        leftDoorHandler.CloseDoor();
        rightDoorHandler.OpenDoor();
        leftTumbleWeedSpawner.SetActive(false);
        leftManaSpawner.SetActive(false);
        rightTumbleWeedSpawner.SetActive(false);
        leftWindParticleSystem.Stop();
        rightWindParticleSystem.Stop();
        
        OnBossEnd.Raise();
    }
    protected override void Start()
    {
        base.Start();
        var player = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter();
        Assert.IsNotNull(player, "PlayerCharacter not found.");
        _playerMovementComponent = player.GetComponent<PlayerMovementComponent>();
        Assert.IsNotNull(_playerMovementComponent, "PlayerMovementComponent not found.");
        Assert.IsNotNull(leftTumbleWeedSpawner, "Left Tumble Weed Spawner not assigned.");
        Assert.IsNotNull(leftManaSpawner, "Left Mana Spawner not assigned.");
        Assert.IsNotNull(rightTumbleWeedSpawner, "Right Tumble Weed Spawner not assigned.");
        Assert.IsNotNull(leftDoorHandler, "Left Door Handler not assigned.");
        Assert.IsNotNull(rightDoorHandler, "Right Door Handler not assigned.");
        Assert.IsNotNull(leftWindParticleSystem, "Left Wind Particle System not assigned.");
        Assert.IsNotNull(rightWindParticleSystem, "Right Wind Particle System not assigned.");
        Assert.IsNotNull(chargeAttackLocationLeft, "Charge Attack Location Left not assigned.");
        Assert.IsNotNull(chargeAttackLocationRight, "Charge Attack Location Right not assigned.");
        Assert.IsNotNull(tornadoPrefab, "Tornado Prefab not assigned.");
        Assert.IsNotNull(TornadoSpawnAttackLocation, "Tornado Spawn Attack Location not assigned.");

    }

    private IEnumerator PerformBossSequence()
    {
        while (true)
        {
            // Perform Wind Attack
            yield return MoveToPosition(leftWindAttackTranform.position, bossSpeed);
            yield return WindAttack(true);

            // Charge 
            yield return ChargeAttack(true);

            // Perform Wind Attack
            yield return MoveToPosition(rightWindAttackTransform.transform.position, bossSpeed);
            yield return WindAttack(false);

            // move to tornado attack location
            yield return MoveToPosition(TornadoSpawnAttackLocation.position, bossSpeed);
            yield return TornadoSpawnAttack();
        }
    }

    private IEnumerator TornadoSpawnAttack()
    {
        // play tornado attack animation
        _animator.SetBool(ANIM_SUMMONING, true);

        float timeElapsed = 0f;
        for(int i = 0; i < numberOfTornadoes; ++i)
        {
            // spawn tornado
            var spawnPos = new Vector3(
                TornadoSpawnAttackLocation.position.x,
                TornadoSpawnAttackLocation.position.y,
                TornadoSpawnAttackLocation.position.z
            );
            GameObject tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.identity);
            _tornadoSummonAttackAudioEvent.Play(tornado);
            Vector2 dir= UnityEngine.Random.insideUnitCircle.normalized;
            Tornado ts = tornado.GetComponent<Tornado>();
            ts.Launch(dir*tornadoSpeed);
            ts.SetLifetime(tornadoAttackDuration - timeElapsed);
            yield return new WaitForSeconds(tornadoSpawnInterval);
            timeElapsed += tornadoSpawnInterval;
        }
        yield return new WaitForSeconds(tornadoAttackDuration - timeElapsed);
        _animator.SetBool(ANIM_SUMMONING, false);
    }

    private IEnumerator MoveToPosition(Vector3 target, float speed)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            SetFacingDirection(target.x < transform.position.x);
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }
    }

    private IEnumerator WindAttack(bool isLeft)
    {
        if(_delayedWindDisableCoroutine != null)
        {
            StopCoroutine(_delayedWindDisableCoroutine);
            _delayedWindDisableCoroutine = null;
            _playerMovementComponent.ResetConstantAcceleration();
        }
        // apply wind push
        var dir = isLeft ? Vector2.right : Vector2.left;
        _playerMovementComponent.AddConstantAcceleration(dir * windPushAccel);

        // play wind attack animation
        _animator.SetBool(ANIM_WIND_ATTACK,true);
        SetFacingDirection(!isLeft);

        if (isLeft)
        {
            leftDoorHandler.OpenDoor();
            leftTumbleWeedSpawner.SetActive(true);
            leftManaSpawner.SetActive(true);
            leftWindParticleSystem.Play();
        }
        else
        {
            rightDoorHandler.OpenDoor();
            rightTumbleWeedSpawner.SetActive(true);
            rightWindParticleSystem.Play();
        }

        // hold the attack
        yield return new WaitForSeconds(windAttackDuration);

        // end wind push
        _delayedWindDisableCoroutine = StartCoroutine(DelayedWindDisable());

        if (isLeft)
        {
            leftDoorHandler.CloseDoor();
            leftTumbleWeedSpawner.SetActive(false);
            leftManaSpawner.SetActive(false);
        }
        else
        {
            rightDoorHandler.CloseDoor();
            rightTumbleWeedSpawner.SetActive(false);
        }
        _animator.SetBool(ANIM_WIND_ATTACK, false);
    }
    private IEnumerator DelayedWindDisable()
    {
        yield return new WaitForSeconds(windPushExtension);
        _playerMovementComponent.ResetConstantAcceleration();
        leftWindParticleSystem.Stop();
        rightWindParticleSystem.Stop();
    }
    private IEnumerator ChargeAttack(bool fromLeft)
    {
        var start = fromLeft
            ? chargeAttackLocationLeft.position
            : chargeAttackLocationRight.position;
        var end = fromLeft
            ? chargeAttackLocationRight.position
            : chargeAttackLocationLeft.position;

        yield return MoveToPosition(start, bossSpeed);
        _animator.SetBool(ANIM_CHARGE, true);
        SetFacingDirection(!fromLeft);
        yield return new WaitForSeconds(chargeAttackDelay);
        _chargeAttackAudioEvent.Play(gameObject);
        yield return MoveToPosition(end, chargeAttackMoveSpeed);
        _animator.SetBool(ANIM_CHARGE, false);

    }
}
