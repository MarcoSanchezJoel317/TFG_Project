using UnityEngine;
using UnityEngine.Audio;
using System;

/// <summary>
/// Gestor único de todas las preferencias del juego:
/// idioma, volúmenes de audio, nivel actual, etc.
/// Se mantiene con DontDestroyOnLoad y dispara eventos al cambiar configuraciones.
/// </summary>
[DefaultExecutionOrder(-50)]
public class SettingsManager : MonoBehaviour
{
    /// <summary>
    /// Instancia global del SettingsManager.
    /// </summary>
    public static SettingsManager Instance { get; private set; }

    /// <summary>
    /// Evento que se dispara cuando cambia el nivel de juego.
    /// El parámetro int es el nuevo nivel.
    /// </summary>
    public static event Action<int> OnLevelChanged;

    [Header("Audio Mixer")]
    [Tooltip("Referencia al AudioMixer con parámetros MasterVolume, MusicVolume y SFXVolume.")]
    public AudioMixer audioMixer;

    private const string PREF_LEVEL = "Level";
    private const string PREF_LANG = "LanguageIndex";
    private const string PREF_MASTER = "MasterVolume";
    private const string PREF_MUSIC = "MusicVolume";
    private const string PREF_SFX = "SFXVolume";

    /// <summary>
    /// Inicializa el singleton, carga el nivel por defecto si es necesario y dispara el evento de nivel.
    /// </summary>
    private void Awake()
    {
        // Singleton y persistencia entre escenas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ajuste inicial de nivel: si no existe, se crea con valor 0
        if (!PlayerPrefs.HasKey(PREF_LEVEL))
        {
            SetYourLevel(1);
        }
        else
        {
            // Disparamos evento con nivel ya guardado
            OnLevelChanged?.Invoke(KnowYourLevel());
        }
    }

    /// <summary>
    /// Carga configuraciones de idioma y audio después de Awake.
    /// </summary>
    private void Start()
    {
        LoadLanguage();
        if (audioMixer != null)
            LoadAudioSettings();
        else
            Debug.LogWarning("SettingsManager: falta asignar AudioMixer.");
    }

    #region Language
    /// <summary>
    /// Lee la preferencia de idioma y la aplica.
    /// </summary>
    private void LoadLanguage()
    {
        if (LanguageManager.Instance != null)
        {
            int langIndex = PlayerPrefs.GetInt(PREF_LANG, 0);
            LanguageManager.Instance.SetLanguage(langIndex);
        }
        else
        {
            Debug.LogWarning("SettingsManager: LanguageManager no inicializado.");
        }
    }
    #endregion

    #region Level Management
    /// <summary>
    /// Obtiene el nivel actual desde PlayerPrefs.
    /// </summary>
    /// <returns>Entero que representa el nivel de juego.</returns>
    public int KnowYourLevel()
    {
        
        return PlayerPrefs.GetInt(PREF_LEVEL, 0);
    }


    /// <summary>
    /// Establece un nuevo nivel de juego, guarda la preferencia y dispara OnLevelChanged.
    /// </summary>
    /// <param name="level">Nivel a guardar (entero).</param>
    public void SetYourLevel(int level)
    {
        PlayerPrefs.SetInt(PREF_LEVEL, level);
        PlayerPrefs.Save();
        OnLevelChanged?.Invoke(level);
    }
    #endregion

    #region Audio Settings
    /// <summary>
    /// Carga valores de volumen (0…1) desde PlayerPrefs y los aplica al AudioMixer.
    /// </summary>
    private void LoadAudioSettings()
    {
        float master = PlayerPrefs.GetFloat(PREF_MASTER, 0.75f);
        float music = PlayerPrefs.GetFloat(PREF_MUSIC, 0.75f);
        float sfx = PlayerPrefs.GetFloat(PREF_SFX, 0.75f);

        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(master, .0001f)) * 20f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(music, .0001f)) * 20f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(sfx, .0001f)) * 20f);
    }

    
    #endregion
}


