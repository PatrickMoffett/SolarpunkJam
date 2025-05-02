using Abilities;
using Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class Mushroom : Enemy
{
    [SerializeField] RangedAttackAbility rangedAttackAbility;
    [SerializeField] float attackRange = 5f;

    private const int newtonIters = 10;
    private const float tolerance = 0.001f;

    private RangedAttackAbility _rangedAttackAbility;
    private GameObject _target;
    private Rigidbody2D _targetRb;
    protected override void Start()
    {
        base.Start();
        Assert.IsNotNull(rangedAttackAbility, $"RangedAttackAbility not assigned in {gameObject.name}.");
        _rangedAttackAbility = Instantiate(rangedAttackAbility);
        _rangedAttackAbility.Initialize(gameObject);

        _target = ServiceLocator.Instance.Get<PlayerManager>().GetPlayerCharacter().gameObject;
        Assert.IsNotNull(_target, $"Player Target not found");
        _targetRb = _target.GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_targetRb, $"Rigidbody2D not found on target {gameObject.name}.");
    }
    private void Update()
    {
        if (Vector2.Distance(transform.position, _target.transform.position) <= attackRange)
        {
            if (_rangedAttackAbility.CanActivate(new AbilityTargetData()))
            {
                Attack();
            }
        }
    }

    private void Attack()
    {
        Rigidbody2D projRb = _rangedAttackAbility
            .GetProjectilePrefab()
            .GetComponent<Rigidbody2D>();

        Vector3 origin = transform.position;
        Vector3 p0 = _targetRb.position;
        Vector3 v0 = _targetRb.linearVelocity;
        Vector3 g = new Vector3(0, Physics2D.gravity.y * projRb.gravityScale, 0);
        float projectileSpeed = _rangedAttackAbility.GetProjectileVelocity();

        // try two initial guesses to get both arcs
        float tGuessLow = (p0 - origin).magnitude / projectileSpeed;
        float tGuessHigh = tGuessLow * 10f;

        float tLow = SolveInterceptTime(origin, p0, v0, g, projectileSpeed, tGuessLow);
        float tHigh = SolveInterceptTime(origin, p0, v0, g, projectileSpeed, tGuessHigh);

        // collect the positive converged solutions
        var candidates = new List<float>();
        if (tLow > 0 && Mathf.Abs(tLow) < 1e6f) candidates.Add(tLow);
        if (tHigh > 0 && Mathf.Abs(tHigh) < 1e6f) candidates.Add(tHigh);

        if (candidates.Count == 0)
        {
            // no valid intercept found
            Debug.LogWarning("Cannot hit target: projectile too slow or out of range.");
            return;
        }

        // pick the largest t -> highest arc
        float t = Mathf.Max(candidates.ToArray());

        // compute launch velocity for that t
        Vector3 pFinal = p0 + v0 * t;
        Vector3 drop = 0.5f * g * t * t;
        Vector3 launch = (pFinal - origin - drop) / t;

        // package and fire
        AbilityTargetData targetData = new AbilityTargetData
        {
            sourceCharacterLocation = origin,
            sourceCharacterDirection = launch
        };
        _rangedAttackAbility.TryActivate(targetData);
    }

    /// <summary>
    /// Solves for t in
    ///   (| (p0 + v0 t -  1/2 g t^2) - origin |)^2 == (vₚ t)^2
    /// via Newton–Raphson with tGuess.
    /// </summary>
    private float SolveInterceptTime(
        Vector3 origin, Vector3 p0, Vector3 v0, Vector3 g,
        float vp, float tGuess
    )
    {
        float t = tGuess;
        for (int i = 0; i < newtonIters; i++)
        {
            // aim vector at time t
            Vector3 aim = (p0 + v0 * t) - origin - 0.5f * g * t * t;

            // f(t) = |aim|^2 - (vp t)^2 = 0
            float f = Vector3.Dot(aim, aim) - (vp * vp) * (t * t);
            if (Mathf.Abs(f) < tolerance)
                break;

            // f'(t) = 2 aim(v0 - g t) - 2 vp^2 t
            Vector3 aimDir = v0 - g * t;
            float df = 2f * (Vector3.Dot(aim, aimDir) - vp * vp * t);
            if (Mathf.Abs(df) < 1e-6f)
                break;

            t -= f / df;
            if (t <= 0f)
                return -1f;
        }
        return t;
    }

    private void OnDrawGizmos()
    {
        // draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}