using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Slider BGMSliderUI;
    public Slider SFXSliderUI;
    public Slider MasterSliderUI;
    public AudioMixer BGM;
    public AudioMixer SFX;
    public AudioMixer Master;
    public void FullScreenToggle(bool enabled)
    {
        Screen.fullScreen = enabled;
    }

    public void BGMSlider(float slider)
    {
        BGM.SetFloat("BGMVolume", slider);
    }

    public void SFXSlider(float slider)
    {
        SFX.SetFloat("SFXVolume", slider);
    }

    public void MasterSlider(float slider)
    {
        Master.SetFloat("MasterVolume", slider);
    }
}
