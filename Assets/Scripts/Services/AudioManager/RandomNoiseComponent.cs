using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class RandomNoisePlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioEvent _audioEvent;
    [SerializeField] private float _minInterval = 5f;
    [SerializeField] private float _maxInterval = 10f;
    private void Start()
    {
        StartCoroutine(PlayRandomNoise());
    }

    private IEnumerator PlayRandomNoise()
    {
        while(true)
        {
            float waitTime = UnityEngine.Random.Range(_minInterval, _maxInterval);
            yield return new WaitForSeconds(waitTime);

            _audioEvent.Play(gameObject);
        }
    }
}