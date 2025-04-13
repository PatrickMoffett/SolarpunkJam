using UnityEngine;

public abstract class AudioEvent : ScriptableObject
{
    public abstract void Play(GameObject gameObject);

    public abstract void Preview(AudioSource previewer);
}