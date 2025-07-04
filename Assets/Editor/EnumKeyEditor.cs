using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class EnumKeyEditor : EditorWindow
{
    string newKey = "";
    const string enumFilePath = "Assets/_Project/_Scripts/_Menu/Language/TextKeys.cs"; // Ajusta la ruta a tu fichero

    [MenuItem("Herramientas/Agregar TextKey")]
    static void ShowWindow()
    {
        GetWindow<EnumKeyEditor>("Agregar TextKey");
    }

    void OnGUI()
    {
        GUILayout.Label("Añadir clave al enum TextKey", EditorStyles.boldLabel);
        newKey = EditorGUILayout.TextField("Nombre de la clave", newKey);

        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newKey));
        if (GUILayout.Button("Agregar"))
        {
            AddEnumValue(newKey);
            newKey = "";
        }
        EditorGUI.EndDisabledGroup();
    }

    void AddEnumValue(string keyName)
    {
        if (!File.Exists(enumFilePath))
        {
            Debug.LogError($"No se encontró {enumFilePath}");
            return;
        }

        string text = File.ReadAllText(enumFilePath);

        // Patrón más laxo: buscamos el último cierre de llave
        var pattern = @"\}\s*$";

        if (!Regex.IsMatch(text, pattern, RegexOptions.Multiline))
        {
            Debug.LogError("No se pudo localizar el cierre del enum TextKey.");
            return;
        }

        // Evitamos duplicados
        if (Regex.IsMatch(text, $@"\b{keyName}\b"))
        {
            Debug.LogWarning($"La clave '{keyName}' ya existe en TextKey.");
            return;
        }

        // Insertamos antes de la última '}'
        text = Regex.Replace(
            text,
            pattern,
            $"    {keyName},\n}}"
        );

        File.WriteAllText(enumFilePath, text);
        AssetDatabase.Refresh();
        Debug.Log($"Clave '{keyName}' agregada a TextKey y proyecto recompilado.");
    }

}

