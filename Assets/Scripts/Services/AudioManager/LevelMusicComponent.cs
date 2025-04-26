using Services;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelMusicComponent : MonoBehaviour
{
    [Tooltip("The audio clip to play when the level starts.")]
    [SerializeField] AudioClip _startingAudioClip;

    private void Start()
    {
        Assert.IsNotNull(_startingAudioClip, "Starting audio clip is not assigned.");
        ServiceLocator.Instance.Get<MusicManager>().StartSong(_startingAudioClip, 0f, true);
    }


}