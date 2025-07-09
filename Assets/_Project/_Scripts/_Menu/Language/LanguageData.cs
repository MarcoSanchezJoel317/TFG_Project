

/// <summary>
/// ScriptableObject que almacena todas las traducciones
/// de un idioma concreto.
/// </summary>
/// 
using UnityEngine;

[System.Serializable]
public struct TextEntry
{
    [Tooltip("Clave que identifica el texto (enum TextKey).")]
    public TextKey key;

    [Tooltip("Texto traducido que aparecerá en pantalla.")]
    public string value; // Ej: "Empezar", "Salir"
}

[CreateAssetMenu(fileName = "LanguageData", menuName = "Idiomas/LanguageData")]
public class LanguageData : ScriptableObject
{
    [Header("Datos de idioma")]
    [Tooltip("Nombre que se mostrará en el selector de idiomas (ej. “Español”).")]
    public string displayName;

    [Tooltip("Listado de parejas (clave, texto) para cada entrada traducible.")]
    public TextEntry[] texts; // Todos los textos aquí

}