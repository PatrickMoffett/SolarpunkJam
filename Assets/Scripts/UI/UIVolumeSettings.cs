using Services;
using UnityEngine;
using UnityEngine.UI;

public class UIVolumeSettings : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public void Setup()
    {
        AudioManager audioManager = ServiceLocator.Instance.Get<AudioManager>();
        masterVolumeSlider.value = audioManager.GetMasterVolume();
        musicVolumeSlider.value = audioManager.GetMusicVolume();
        sfxVolumeSlider.value = audioManager.GetSfxVolume();
        
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void SetMasterVolume(float value)
    {
        ServiceLocator.Instance.Get<AudioManager>().SetMasterVolume(value);
    }

    void SetMusicVolume(float value)
    {
        ServiceLocator.Instance.Get<AudioManager>().SetMusicVolume(value);
    }

    void SetSFXVolume(float value)
    {
        ServiceLocator.Instance.Get<AudioManager>().SetSfxVolume(value);
    }

    public void ExitSettingsMenuClicked()
    {
        ServiceLocator.Instance.Get<ApplicationStateManager>().PopState();
    }
}