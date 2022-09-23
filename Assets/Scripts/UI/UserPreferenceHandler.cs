using UnityEngine;
using Survivor.Core;

public class UserPreferenceHandler : MonoBehaviour
{
    [SerializeField] private UserPreference userPreference;
    [SerializeField] private AdvancedSlider advSliderMasterVolume;
    [SerializeField] private AdvancedSlider advSliderMusicVolume;
    [SerializeField] private AdvancedSlider advSliderSfxVolume;
    [SerializeField] private AdvancedSlider advSliderAmbientVolume;

    private void Awake() 
    {
        userPreference.LoadUniversalData();
        ReadFromUserPreference();
    }

    private void ReadFromUserPreference() 
    {
        advSliderMasterVolume.slider.value = userPreference.masterVolume;
        advSliderMusicVolume.slider.value = userPreference.musicVolume;
        advSliderSfxVolume.slider.value = userPreference.sfxVolume;
        advSliderAmbientVolume.slider.value = userPreference.ambientVolume;

        advSliderMasterVolume.input.text = userPreference.masterVolume.ToString();
        advSliderMusicVolume.input.text = userPreference.musicVolume.ToString();
        advSliderSfxVolume.input.text = userPreference.sfxVolume.ToString();
        advSliderAmbientVolume.input.text = userPreference.ambientVolume.ToString();
    }

    public void WriteToUserPreference() 
    {
        userPreference.masterVolume = advSliderMasterVolume.slider.value;
        userPreference.musicVolume = advSliderMusicVolume.slider.value;
        userPreference.sfxVolume = advSliderSfxVolume.slider.value;
        userPreference.ambientVolume = advSliderAmbientVolume.slider.value;
        AudioManager.Instance.UpdateVolumes();
    } 
}
