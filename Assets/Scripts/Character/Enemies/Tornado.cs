using System.Collections;
using UnityEngine;

public class Tornado : Enemy
{
    [Header("Events")]
    [SerializeField] private GameEvent BossFightEnded;

    [Header("Fade")]
    [Tooltip("Duration (in seconds) of the fade-out at the end of life.")]
    [SerializeField] private float fadeDuration = .5f;

    private Coroutine _lifetimeCoroutine;

    protected override void Start()
    {
        base.Start();

        BossFightEnded.OnGameEvent += OnBossFightEnded;

    }

    private void OnDestroy()
    {
        BossFightEnded.OnGameEvent -= OnBossFightEnded;
    }

    private void OnBossFightEnded()
    {
        if (_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);

        _lifetimeCoroutine = StartCoroutine(FadeOutAndDestroy(fadeDuration));
    }


    public void Launch(Vector2 velocity)
    {
        _rigidbody2D.linearVelocity = velocity;
    }

    public void SetLifetime(float newLifetime)
    {
        if (_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);
        _lifetimeCoroutine = StartCoroutine(FadeOutAndDestroy(newLifetime));
    }

    private IEnumerator FadeOutAndDestroy(float totalLife)
    {
        float elapsed = 0f;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No spriteRenderer on Tornado");
            Destroy(gameObject, totalLife);
        }
        else
        {
            Color original = spriteRenderer.color;

            while (elapsed < totalLife)
            {
                elapsed += Time.deltaTime;

                // once we're in the final fadeDuration window, lerp alpha
                if (elapsed >= totalLife - fadeDuration)
                {
                    float t = (elapsed - (totalLife - fadeDuration)) / fadeDuration;
                    var c = original;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    spriteRenderer.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
