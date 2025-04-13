using System.Collections;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// A class for managing the playing of music tracks.
    /// </summary>
    public class MusicManager : IService
    {
        /// <summary>
        /// The AudioSource of the CurrentSong
        /// </summary>
        private AudioSource _currentSongSource;

        private Coroutine _fadeInCoroutine;

        public MusicManager() { }

        ~MusicManager() { }

        /// <summary>
        /// Starts playing a new music track, fading out the previous track if one is playing.
        /// </summary>
        /// <param name="songToPlay">The AudioClip to play.</param>
        /// <param name="transitionTime">The amount of time (in seconds) to take to transition between tracks.</param>
        /// <param name="forceRestartSong">If true, the track will be restarted from the beginning even if it is already playing.</param>
        public void StartSong(AudioClip songToPlay, float transitionTime, bool forceRestartSong = false)
        {
            if (!forceRestartSong && _currentSongSource && songToPlay == _currentSongSource.clip)
            {
                return;
            }

            if (_currentSongSource == null)
            {
                _currentSongSource = ServiceLocator.Instance.Get<AudioManager>().GetMusicAudioSource();
            }
            else
            {
                if (_currentSongSource.isPlaying)
                {
                    if (_fadeInCoroutine != null)
                    {
                        ServiceLocator.Instance.Get<MonoBehaviorService>().StopCoroutine(_fadeInCoroutine);
                    }
                    ServiceLocator.Instance.Get<MonoBehaviorService>()
                        .StartCoroutine(FadeOutAndStopSong(_currentSongSource, transitionTime));
                    //get new Audio Source
                    _currentSongSource = ServiceLocator.Instance.Get<AudioManager>().GetMusicAudioSource();
                }
            }

            _currentSongSource.clip = songToPlay;
            _currentSongSource.loop = true;
            if (transitionTime != 0f)
            {
                _fadeInCoroutine = ServiceLocator.Instance.Get<MonoBehaviorService>()
                    .StartCoroutine(PlayAndFadeIn(_currentSongSource, transitionTime));
            }
            else
            {
                _currentSongSource.volume = 1f;
            }

            _currentSongSource.Play();
        }

        /// <summary>
        /// Fades out and stops the specified audio source. Destroys the source when finished.
        /// </summary>
        /// <param name="sourceToFade">The audio source to fade out and stop.</param>
        /// <param name="fadeTime">The time it takes for the fade out effect to complete.</param>
        /// <returns>An <see cref="IEnumerator"/> used for coroutine.</returns>
        IEnumerator FadeOutAndStopSong(AudioSource sourceToFade, float fadeTime)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                sourceToFade.volume = 1 - (elapsedTime / fadeTime);
                yield return null;
            }

            sourceToFade.volume = 0;
            Object.Destroy(sourceToFade);
        }

        /// <summary>
        /// Fades in the specified audio source and starts playing it.
        /// </summary>
        /// <param name="sourceToFade">The audio source to fade in and start playing.</param>
        /// <param name="fadeTime">The time it takes for the fade in effect to complete.</param>
        /// <returns>An <see cref="IEnumerator"/> used for coroutine.</returns>
        IEnumerator PlayAndFadeIn(AudioSource sourceToFade, float fadeTime)
        {
            sourceToFade.volume = 0f;
            float elapsedTime = 0f;
            sourceToFade.Play();
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                sourceToFade.volume = elapsedTime / fadeTime;
                yield return null;
            }

            sourceToFade.volume = 1;
        }

    }
}