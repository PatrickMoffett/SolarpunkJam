using System;
using Services;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


[CreateAssetMenu(menuName="Audio Events/Simple")]
public class SimpleAudioEvent : AudioEvent , IResetOnExitPlay
{
    public AudioClip[] clips;

    public RangedFloat volume;

    [MinMaxRange(0, 2)]
    public RangedFloat pitch;

    [Range(0f,1f)]
    public float spatialBlend = 1f;

    public float minimumReplayTime = 0f;

    private float _lastTimePlayed = 0f;
    
    private void OnEnable()
    {
        _lastTimePlayed = 0f;
    }

    private void OnDisable()
    {
        _lastTimePlayed = 0f;
    }

    public override void Play(GameObject gameObject)
    {
        if (clips.Length == 0) return;

        if (Time.fixedUnscaledTime - _lastTimePlayed < minimumReplayTime) return;
        _lastTimePlayed = Time.fixedUnscaledTime;
        
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float volume_ = Random.Range(volume.minValue, volume.maxValue);
        float pitch_ = Random.Range(pitch.minValue, pitch.maxValue);
        
        GameObject sfx = ServiceLocator.Instance.Get<AudioManager>()
            .PlaySfxAtLocation(clip, gameObject.transform.position, volume_, pitch_, spatialBlend)
            .gameObject;

        sfx.transform.parent = gameObject.transform;
        
    }

    public override void Preview(AudioSource previewer)
    {
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float volume_ = Random.Range(volume.minValue, volume.maxValue);
        float pitch_ = Random.Range(pitch.minValue, pitch.maxValue);
        
        previewer.clip = clip;
        previewer.volume = volume_;
        previewer.pitch = pitch_;
        previewer.spatialBlend = spatialBlend;
        previewer.Play();
    }

    public void ResetOnExitPlay()
    {
        _lastTimePlayed = 0f;
    }
}