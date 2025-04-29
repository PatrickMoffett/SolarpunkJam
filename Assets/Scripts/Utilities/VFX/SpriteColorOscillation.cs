using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteColorOscillator : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Coroutine _currentRoutine;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    /// <summary>
    /// Starts oscillating between colorA and colorB over duration seconds at frequency cycles/sec.
    /// </summary>
    public void StartSpriteOscillation(Color a, Color b, float duration, float frequency)
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        _currentRoutine = StartCoroutine(Oscillate(a, b, duration, frequency));
    }

    private IEnumerator Oscillate(Color a, Color b, float duration, float frequency)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * frequency, 1f);
            _spriteRenderer.color = Color.Lerp(a, b, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // restore original color
        _spriteRenderer.color = _originalColor;
        _currentRoutine = null;
    }
}
