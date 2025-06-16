// LanguageData.cs

/*
 Define un ScriptableObject que almacena textos traducidos para un idioma.

[CreateAssetMenu]: Permite crear assets de idiomas desde el menú de Unity (clic derecho en el proyecto → Create → Idiomas → LanguageData).

Campos públicos: Cada variable corresponde a un texto del UI (ej: start es el texto del botón "Empezar").
*/

using UnityEngine;

[System.Serializable]
public struct TextEntry
{
    public TextKey key;
    public string value; // Ej: "Empezar", "Salir"
}

[CreateAssetMenu(fileName = "LanguageData", menuName = "Idiomas/LanguageData")]
public class LanguageData : ScriptableObject
{
    public string displayName; // "Español", "English"
    public TextEntry[] texts; // Todos los textos aquí

}