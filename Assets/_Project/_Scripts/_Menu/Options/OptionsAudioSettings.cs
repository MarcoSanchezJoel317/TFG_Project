using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsAudioSettings : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    void Start()
    {
        // Carga valores guardados o 0.75f por defecto
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // Aplica de entrada
        ApplyMaster(masterSlider.value);
        ApplyMusic(musicSlider.value);
        ApplySfx(sfxSlider.value);

        // Suscripción a cambios
        masterSlider.onValueChanged.AddListener(ApplyMaster);
        musicSlider.onValueChanged.AddListener(ApplyMusic);
        sfxSlider.onValueChanged.AddListener(ApplySfx);
    }

    void ApplyMaster(float v)
    {
        float dB;
        if (v <= 0f)
            dB = -80f;               // Nivel “silencio” en Unity suele considerarse -80 dB
        else
            dB = Mathf.Log10(v) * 20f;

        audioMixer.SetFloat("MasterVolume", dB);
        PlayerPrefs.SetFloat("MasterVolume", v);
    }

    void ApplyMusic(float v)
    {
        float dB = (v <= 0f) ? -80f : Mathf.Log10(v) * 20f;
        audioMixer.SetFloat("MusicVolume", dB);
        PlayerPrefs.SetFloat("MusicVolume", v);
    }

    void ApplySfx(float v)
    {
        float dB = (v <= 0f) ? -80f : Mathf.Log10(v) * 20f;
        audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", v);
    }

}

