using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Singleton que gestiona la localización del juego.
///  Carga traducciones desde ScriptableObjects.
///  Dispara OnLanguageChanged cuando cambia el idioma.
/// </summary>
/// <version>1.1 – 2025-06-13</version>
[DefaultExecutionOrder(-100)]
public class LanguageManager : MonoBehaviour
{
    [Tooltip("Asignar un LanguageData por cada idioma soportado.")]
    [SerializeField] private LanguageData[] _languages;

    private int _currentLanguageIndex = 0;
    private Dictionary<TextKey, string> _textLookup = new Dictionary<TextKey, string>();

    public static LanguageManager Instance { get; private set; }
    public event Action OnLanguageChanged;

    #region Ciclo de vida

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Validación de configuración mínima
        if (_languages == null || _languages.Length == 0)
            Debug.LogError("[LanguageManager] No hay LanguageData asignados.");

        // No hacemos SetLanguage aquí: lo invocará SettingsManager en su Start.
    }

    #endregion

    #region API pública

    /// <summary>
    /// Cambia el idioma activo y reconstruye el diccionario interno.
    /// </summary>
    /// <param name="index">Índice del idioma en el array _languages.</param>
    public void SetLanguage(int index)
    {
        // Clamp para evitar índices inválidos
        _currentLanguageIndex = Mathf.Clamp(index, 0, _languages.Length - 1);

        // Guardamos la elección
        PlayerPrefs.SetInt("LanguageIndex", _currentLanguageIndex);
        PlayerPrefs.Save();

        // Reconstruimos el diccionario
        _textLookup = _languages[_currentLanguageIndex]
            .texts
            .ToDictionary(entry => entry.key, entry => entry.value);

        // Notificamos a todos los suscriptores
        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Recupera el texto traducido para la clave indicada.
    /// </summary>
    /// <param name="key">Clave definida en TextKey.</param>
    /// <returns>Texto traducido, o el nombre de la clave si falta traducción.</returns>
    public string GetText(TextKey key)
    {
        if (_textLookup.TryGetValue(key, out var value))
            return value;

        Debug.LogWarning($"[LanguageManager] Falta traducción para la clave: {key}");
        return key.ToString();
    }

    /// <summary>
    /// Índice del idioma actualmente activo (0-based).
    /// </summary>
    public int GetCurrentLanguageIndex() => _currentLanguageIndex;

    /// <summary>
    /// Lista de idiomas disponibles (lectura).
    /// </summary>
    public IReadOnlyList<LanguageData> AvailableLanguages => _languages;

    #endregion
}


