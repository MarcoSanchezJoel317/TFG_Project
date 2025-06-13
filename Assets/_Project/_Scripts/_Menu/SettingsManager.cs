using UnityEngine;
using UnityEngine.Audio;
using System;

/// <summary>
/// Gestor único de todas las preferencias del juego:
/// idioma, volúmenes de audio, etc.
/// Se mantiene con DontDestroyOnLoad y carga/aplica todo en Awake.
/// </summary>
[DefaultExecutionOrder(-50)]

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    void Awake()
    {
        // Singleton y DontDestroyOnLoad...
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(KnowYourLevel()  == 1)
        {
            SetYourLevel(2);
        }

    }

    void Start()
    {
        // Ahora LanguageManager ya habrá hecho su Awake()
        LoadLanguage();
        if (audioMixer != null)
            LoadAudioSettings();
        else
            Debug.LogWarning("SettingsManager: falta asignar AudioMixer.");
    }



    #region Language
    private void LoadLanguage()
    {
        if (LanguageManager.Instance != null)
        {
            int langIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
            LanguageManager.Instance.SetLanguage(langIndex);
        }
        else
        {
            Debug.LogWarning("SettingsManager: LanguageManager todavía no inicializado.");
        }
    }

    #endregion

    #region KnowYourLevel

    public int KnowYourLevel()
    {
        int level = PlayerPrefs.GetInt("Level", 1);

        return level;
    }
    
    public void SetYourLevel(int level)
    {
        PlayerPrefs.SetFloat("Level", level);
        PlayerPrefs.Save();
    }


    #endregion

    #region Audio
    private void LoadAudioSettings()
    {
        // Recuperamos valores guardados (0…1)
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Los convertimos a decibelios y aplicamos al AudioMixer
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(master, .0001f)) * 20f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(music, .0001f)) * 20f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(sfx, .0001f)) * 20f);
    }
    #endregion

    // --- Si quieres exponer métodos para cambiar ajustes---
    public void SetMasterVolume(float v)
    {
        PlayerPrefs.SetFloat("MasterVolume", v);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(v, .0001f)) * 20f);
    }
    public void SetMusicVolume(float v)
    {
        PlayerPrefs.SetFloat("MusicVolume", v);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(v, .0001f)) * 20f);
    }
    public void SetSfxVolume(float v)
    {
        PlayerPrefs.SetFloat("SFXVolume", v);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(v, .0001f)) * 20f);
    }
}

