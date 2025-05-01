using UnityEngine;

public abstract class AudioEvent : ScriptableObject
{
    public abstract float Play(GameObject gameObject);

    public abstract void Preview(AudioSource previewer);

}