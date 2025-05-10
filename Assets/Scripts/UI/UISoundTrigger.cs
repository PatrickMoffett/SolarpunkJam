using UnityEngine;

public class UISoundTrigger : MonoBehaviour
{
    [SerializeField] private SimpleAudioEvent _soundEvent;

    public void PlaySound()
    {
        if (_soundEvent)
        {
            _soundEvent.Play();
        }
    }
}
