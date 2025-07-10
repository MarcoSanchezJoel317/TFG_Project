using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Gestiona las opciones de audio desde sliders en el menú:
/// carga valores guardados, aplica cambios al AudioMixer y persiste en PlayerPrefs.
/// </summary>
public class OptionsAudioSettings : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Slider de volumen master (0…1).")]
    [SerializeField] private Slider _masterSlider;
    [Tooltip("Slider de volumen de música (0…1).")]
    [SerializeField] private Slider _musicSlider;
    [Tooltip("Slider de volumen de efectos (0…1).")]
    [SerializeField] private Slider _sfxSlider;

    [Header("Audio Mixer")]
    [Tooltip("AudioMixer con parámetros MasterVolume, MusicVolume, SFXVolume.")]
    [SerializeField] private AudioMixer _audioMixer;

    private const string PREF_MASTER = "MasterVolume";
    private const string PREF_MUSIC = "MusicVolume";
    private const string PREF_SFX = "SFXVolume";

    private void Awake()
    {
        // Validaciones básicas
        if (_masterSlider == null || _musicSlider == null || _sfxSlider == null || _audioMixer == null)
        {
            Debug.LogError("[OptionsAudioSettings] Faltan referencias en el Inspector.");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Inicializa sliders con valores de PlayerPrefs y suscribe listeners.
    /// </summary>
    private void OnEnable()
    {
        // Carga valores guardados o default 0.75f
        _masterSlider.value = PlayerPrefs.GetFloat(PREF_MASTER, 0.75f);
        _musicSlider.value = PlayerPrefs.GetFloat(PREF_MUSIC, 0.75f);
        _sfxSlider.value = PlayerPrefs.GetFloat(PREF_SFX, 0.75f);

        // Aplica inmediatamente
        ApplyVolume(PREF_MASTER, "MasterVolume", _masterSlider.value);
        ApplyVolume(PREF_MUSIC, "MusicVolume", _musicSlider.value);
        ApplyVolume(PREF_SFX, "SFXVolume", _sfxSlider.value);

        // Listeners de UI
        _masterSlider.onValueChanged.AddListener(v => ApplyVolume(PREF_MASTER, "MasterVolume", v));
        _musicSlider.onValueChanged.AddListener(v => ApplyVolume(PREF_MUSIC, "MusicVolume", v));
        _sfxSlider.onValueChanged.AddListener(v => ApplyVolume(PREF_SFX, "SFXVolume", v));
    }

    /// <summary>
    /// Desuscribe los listeners al desactivarse el objeto.
    /// </summary>
    private void OnDisable()
    {
        _masterSlider.onValueChanged.RemoveAllListeners();
        _musicSlider.onValueChanged.RemoveAllListeners();
        _sfxSlider.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    /// Convierte un valor [0…1] a dB, lo aplica al AudioMixer y lo guarda en PlayerPrefs.
    /// </summary>
    /// <param name="prefKey">Clave en PlayerPrefs (p. ej. \"MusicVolume\").</param>
    /// <param name="mixerParam">Nombre del parámetro en el AudioMixer.</param>
    /// <param name="value">Valor de volumen 0…1.</param>
    private void ApplyVolume(string prefKey, string mixerParam, float value)
    {
        // Conversión lineal a decibelios; Unity trata -80dB como silencio
        float dB = (value <= 0f) ? -80f : Mathf.Log10(value) * 20f;

        _audioMixer.SetFloat(mixerParam, dB);
        PlayerPrefs.SetFloat(prefKey, value);
        PlayerPrefs.Save();
    }
}


