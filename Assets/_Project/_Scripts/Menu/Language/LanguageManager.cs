using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance; // Patrón Singleton

    public LanguageData[] languages; // Array de idiomas (ES, EN, etc.)
    private int _currentLanguageIndex = 0;

    // Cambia el idioma (ej: desde el dropdown)
    private Dictionary<TextKey, string> _textLookup; // Diccionario para textos

    private void Start()
    {
        _currentLanguageIndex = PlayerPrefs.GetInt("LanguageIndex", 0); // Carga el último idioma (0 por defecto)
        SetLanguage(_currentLanguageIndex);
    }

    private void Awake()
    {
        // Singleton: Solo una instancia en todo el juego
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject); // Destruye duplicados
        }
    }

    

    public void SetLanguage(int index)
    {
        _currentLanguageIndex = Mathf.Clamp(index, 0, languages.Length - 1);
        PlayerPrefs.SetInt("LanguageIndex", _currentLanguageIndex);
        PlayerPrefs.Save();

        // Llenar el diccionario
        _textLookup = new Dictionary<TextKey, string>();
        foreach (var entry in languages[_currentLanguageIndex].texts)
        {
            _textLookup[entry.key] = entry.value;
        }
    }
    // Obtiene el texto traducido (ej: "start" → "Empezar")

    public string GetText(TextKey key)
    {
        if (_textLookup.TryGetValue(key, out string value))
            return value;

        Debug.LogError($"Clave '{key}' no encontrada.");
        return "ERROR";
    }


    public int GetCurrentLanguageIndex()
    {
        return _currentLanguageIndex;
    }
}
