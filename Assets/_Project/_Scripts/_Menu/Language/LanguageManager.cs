using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Singleton que gestiona la localización del juego.
/// Carga traducciones desde LanguageData y notifica cambios de idioma.
/// </summary>
[DefaultExecutionOrder(-100)]
public class LanguageManager : MonoBehaviour
{
    [Header("Assets de idioma")]
    [Tooltip("Asignar un LanguageData por cada idioma soportado.")]
    [SerializeField] private LanguageData[] _languages;

    private const string PREF_LANG = "LanguageIndex";
    private int _currentLanguageIndex = 0;
    private Dictionary<TextKey, string> _textLookup = new();

    public static LanguageManager Instance { get; private set; }

    /// <summary>
    /// Se dispara cuando cambia el idioma activo.
    /// </summary>
    public event Action OnLanguageChanged;

    private void Awake()
    {
        // Singleton y persistencia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Validación básica
        if (_languages == null || _languages.Length == 0)
            Debug.LogError("[LanguageManager] No hay LanguageData asignados.");
    }

    private void Start()
    {
        // Carga el idioma guardado (o 0 si no existe)
        int saved = PlayerPrefs.GetInt(PREF_LANG, 0);
        SetLanguage(saved);
    }

    /// <summary>
    /// Cambia el idioma y reconstruye el diccionario interno.
    /// </summary>
    /// <param name="index">Índice en _languages (clamp entre 0 y Length-1).</param>
    public void SetLanguage(int index)
    {
        _currentLanguageIndex = Mathf.Clamp(index, 0, _languages.Length - 1);
        PlayerPrefs.SetInt(PREF_LANG, _currentLanguageIndex);
        PlayerPrefs.Save();

        _textLookup = _languages[_currentLanguageIndex]
            .texts.ToDictionary(e => e.key, e => e.value);

        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Obtiene el texto traducido para la clave dada.
    /// </summary>
    /// <param name="key">Clave TextKey.</param>
    /// <returns>Texto en el idioma actual o nombre de la clave si falta.</returns>
    public string GetText(TextKey key)
    {
        if (_textLookup.TryGetValue(key, out var value))
            return value;

        Debug.LogWarning($"[LanguageManager] Falta traducción para: {key}");
        return key.ToString();
    }

    /// <summary>
    /// Índice del idioma actualmente activo (0-based).
    /// </summary>
    public int GetCurrentLanguageIndex() => _currentLanguageIndex;

    /// <summary>
    /// Lista de LanguageData disponibles (solo lectura).
    /// </summary>
    public IReadOnlyList<LanguageData> AvailableLanguages => _languages;
}



