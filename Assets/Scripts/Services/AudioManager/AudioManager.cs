using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Services
{
    /// <summary>
    /// The AudioManager class is responsible for managing the game's audio. 
    /// It contains methods for getting and setting the master, music, and sound effect (SFX) volumes,
    /// as well as playing SFX and music audio clips.
    /// It implements the IService interface so that it may be held by the ServiceLocator
    /// <br/><br/>
    /// Global Access to the AudioManager is available from the ServiceLocator<br/>
    /// How to Access: ServiceLocator.Instance.Get&lt;AudioManager&gt;();
    /// </summary>
    public class AudioManager : IService
    {

        //These are setting parameters stored in a ScriptableObject under "Assets/Settings/AudioSetting/AudioManagerParams"
        private readonly AudioManagerParams _params = Resources.Load<AudioManagerParams>("AudioManagerParams");
        
        private readonly AudioMixer _mixer;
        private readonly AudioSource _sfxAudioSource;
        private readonly GameObject _audioManagerGameObject;
        public AudioManager()
        {
            //get mixer from settings
            _mixer = _params.mixer;
            
            // Create a new GameObject to hold audioSources for us
            _audioManagerGameObject = new GameObject("AudioManagerAudioSource");
            Object.DontDestroyOnLoad(_audioManagerGameObject);
            
            //Create a new SFX audio source for playing non-positional SFX
            _sfxAudioSource = _audioManagerGameObject.AddComponent<AudioSource>();
            _sfxAudioSource.outputAudioMixerGroup = _params.sfxGroup;
        }

        /// <summary>
        /// 
        /// </summary>
        ~AudioManager()
        {
            Object.Destroy(_audioManagerGameObject);
        }
        /// <summary>
        /// Gets the current master volume level from the audio mixer.
        /// </summary>
        /// <returns>The current master volume level as a percentage between 0 and 1.</returns>
        public float GetMasterVolume()
        {
            _mixer.GetFloat("MasterVolume", out float volume);
            return DecibelToLinear(volume);
        }
        
        /// <summary>
        /// Gets the current music volume level from the audio mixer.
        /// </summary>
        /// <returns>The current music volume level as a percentage between 0 and 1.</returns>
        public float GetMusicVolume()
        {
            _mixer.GetFloat("MusicVolume", out float volume);
            return DecibelToLinear(volume);
        }
        
        /// <summary>
        /// Gets the current SFX volume level from the audio mixer.
        /// </summary>
        /// <returns>The current SFX volume level as a percentage between 0 and 1.</returns>
        public float GetSfxVolume()
        {
            _mixer.GetFloat("SFXVolume", out float volume);
            return DecibelToLinear(volume);
        }
        /// <summary>
        /// Sets the master volume level in the audio mixer.
        /// input clamped between 0-1
        /// </summary>
        /// <param name="newVal">The new master volume level.</param>
        public void SetMasterVolume(float newVal)
        {
            newVal = Mathf.Clamp(newVal, 0f, 1f);
            newVal = LinearToDecibel(newVal);
            _mixer.SetFloat("MasterVolume", newVal);
        }
        
        /// <summary>
        /// Sets the music volume level in the audio mixer.
        /// input clamped between 0-1
        /// </summary>
        /// <param name="newVal">The new music volume level.</param>
        public void SetMusicVolume(float newVal)
        {
            newVal = Mathf.Clamp(newVal, 0f, 1f);
            newVal = LinearToDecibel(newVal);
            _mixer.SetFloat("MusicVolume", newVal);
        }
        
        /// <summary>
        /// Sets the SFX volume level in the audio mixer.
        /// input clamped between 0-1
        /// </summary>
        /// <param name="newVal">The new SFX volume level.</param>
        public void SetSfxVolume(float newVal)
        {
            newVal = Mathf.Clamp(newVal, 0f, 1f);
            newVal = LinearToDecibel(newVal);
            _mixer.SetFloat("SFXVolume", newVal);
        }
        
        /// <summary>
        /// Converts a linear value to its corresponding decibel value.
        /// </summary>
        /// <param name="linear">The linear value to convert.</param>
        /// <returns>The decibel value.</returns>
        private float LinearToDecibel(float linear)
        {
            float dB;

            if (linear != 0)
                dB = 20.0f * Mathf.Log10(linear);
            else
                dB = -144.0f;

            return dB;
        }
        
        
        /// <summary>
        /// Converts a decibel value to its corresponding linear value.
        /// </summary>
        /// <param name="dB">The decibel value to convert.</param>
        /// <returns>The linear value.</returns>
        private float DecibelToLinear(float dB)
        {
            float linear = Mathf.Pow(10.0f, dB / 20.0f);

            return linear;
        }

        /// <summary>
        /// Plays a sound effect at the specified location in the game world.
        /// </summary>
        /// <param name="clipToPlay">The AudioClip to play.</param>
        /// <param name="location">The position in the game world where the sound effect should be played.</param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="spatialBlend"></param>
        /// <returns>The AudioSource component used to play the sound effect.</returns>
        public AudioSource PlaySfxAtLocation(AudioClip clipToPlay, Vector3 location, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
        {
            GameObject temp = new GameObject("SFX");
            temp.transform.position = location;
            AudioSource audioSource = temp.AddComponent<AudioSource>();
            if(!CheckIfOnScreen(location))
            {
                return audioSource;
            }
            audioSource.outputAudioMixerGroup = _params.sfxGroup;
            audioSource.clip = clipToPlay;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialize = true;
            audioSource.spatialBlend = spatialBlend;
            audioSource.Play();
            Object.Destroy(audioSource.gameObject,clipToPlay.length);
            return audioSource;
        }
        private bool CheckIfOnScreen(Vector3 position)
        {
            if (Camera.main == null)
            {
                return false;
            }
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(position);
            return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
        }
        /// <summary>
        /// Plays a sound effect once using the SFX output mixer group.
        /// </summary>
        /// <param name="clipToPlay">The AudioClip to play.</param>
        public void PlaySfx(AudioClip clipToPlay, float volume = 1f, float pitch = 1f)
        {
            _sfxAudioSource.pitch = pitch;
            _sfxAudioSource.PlayOneShot(clipToPlay, volume);
        }
        
        /// <summary>
        /// Returns a new AudioSource component attached to the AudioManager's GameObject with the music output mixer group.
        /// </summary>
        /// <returns>New AudioSource component with the music output mixer group.</returns>
        public AudioSource GetMusicAudioSource()
        {
            AudioSource newSource = _audioManagerGameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = _params.musicGroup;
            return newSource;
        }
    }
}