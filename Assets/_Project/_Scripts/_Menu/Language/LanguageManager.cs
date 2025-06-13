// LanguageManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gestor principal de localización.  
/// Se mantiene como singleton para ofrecer acceso global
/// y dispara un evento cuando cambia el idioma.
/// </summary>

[DefaultExecutionOrder(-100)]

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [Tooltip("Assets de tipo LanguageData (ScriptableObject) con traducciones para cada idioma.")]
    public LanguageData[] languages;

    private int _currentLanguageIndex;
    private Dictionary<TextKey, string> _textLookup;

    /// <summary>
    /// Evento que notifica a los suscriptores que se ha cambiado el idioma.
    /// </summary>
    public event Action OnLanguageChanged;

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //// Carga la configuración guardada o establece por defecto el primer idioma
        //_currentLanguageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        //SetLanguage(_currentLanguageIndex);
    }

    /// <summary>
    /// Cambia el idioma activo, actualiza PlayerPrefs y reconstruye el diccionario interno.
    /// </summary>
    /// <param name="index">Índice del array 'languages' que corresponde al idioma deseado.</param>
    public void SetLanguage(int index)
    {
        // Asegura que el índice esté en rango
        _currentLanguageIndex = Mathf.Clamp(index, 0, languages.Length - 1);

        // Guarda la elección para futuras sesiones
        PlayerPrefs.SetInt("LanguageIndex", _currentLanguageIndex);
        PlayerPrefs.Save();

        // Reconstruye el diccionario a partir del ScriptableObject correspondiente
        _textLookup = languages[_currentLanguageIndex]
            .texts
            .ToDictionary(entry => entry.key, entry => entry.value);

        // Notifica a todos los oyentes que el idioma ha cambiado
        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Recupera la cadena traducida para la clave dada.
    /// En caso de fallo, devuelve el nombre de la clave y avisa con un warning.
    /// </summary>
    public string GetText(TextKey key)
    {
        if (_textLookup != null && _textLookup.TryGetValue(key, out var value))
            return value;

        Debug.LogWarning($"Localization: falta traducción para la clave '{key}'.");
        return key.ToString();
    }

    /// <summary>
    /// Índice del idioma actualmente activo.
    /// </summary>
    public int GetCurrentLanguageIndex() => _currentLanguageIndex;
}

