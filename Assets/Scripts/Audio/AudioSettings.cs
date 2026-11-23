using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Sliders UI")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // 1. Cargar los valores guardados (o 0.75 si es la primera vez)
        float masterVal = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        float musicVal = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfxVal = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        // 2. Posicionar visualmente los sliders
        masterSlider.value = masterVal;
        musicSlider.value = musicVal;
        sfxSlider.value = sfxVal;

        // 3. Aplicar los volúmenes al AudioMixer al iniciar
        SetMasterVolume(masterVal);
        SetMusicVolume(musicVal);
        SetSFXVolume(sfxVal);

        // 4. Escuchar cuando el jugador mueve cada slider
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat("MasterVol", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat("MasterVol", value);
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat("SFXVol", value);
    }

    
}
