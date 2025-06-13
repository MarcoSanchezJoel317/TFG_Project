// Editor/LanguageDataEditor.cs
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(LanguageData))]
public class LanguageDataEditor : Editor
{
    SerializedProperty displayNameProp;
    SerializedProperty textsProp;
    LanguageData data;

    void OnEnable()
    {
        data = (LanguageData)target;
        displayNameProp = serializedObject.FindProperty(nameof(LanguageData.displayName));
        textsProp = serializedObject.FindProperty(nameof(LanguageData.texts));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Campo para el nombre a mostrar
        EditorGUILayout.PropertyField(displayNameProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Traducciones", EditorStyles.boldLabel);

        // Recorremos todas las claves en el enum TextKey
        foreach (TextKey key in System.Enum.GetValues(typeof(TextKey)))
        {
            // Tratamos de encontrar el índice actual en el array
            int idx = data.texts.ToList().FindIndex(e => e.key == key);

            string currentValue = (idx >= 0) ? data.texts[idx].value : "";

            // Dibujamos un TextField para este key
            string newValue = EditorGUILayout.TextField(key.ToString(), currentValue);

            // Si cambió el valor, lo actualizamos en el array serializado
            if (newValue != currentValue)
            {
                if (idx >= 0)
                {
                    // Actualizamos entrada existente
                    SerializedProperty entryProp = textsProp.GetArrayElementAtIndex(idx);
                    entryProp.FindPropertyRelative("value").stringValue = newValue;
                }
                else
                {
                    // Insertamos nueva entrada al final
                    textsProp.InsertArrayElementAtIndex(textsProp.arraySize);
                    SerializedProperty newEntry = textsProp.GetArrayElementAtIndex(textsProp.arraySize - 1);
                    newEntry.FindPropertyRelative("key").enumValueIndex = (int)key;
                    newEntry.FindPropertyRelative("value").stringValue = newValue;
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Eliminar entradas vacías"))
        {
            // Opcional: limpiar las entradas cuyo valor esté vacío
            for (int i = textsProp.arraySize - 1; i >= 0; i--)
            {
                var entry = textsProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(entry.FindPropertyRelative("value").stringValue))
                    textsProp.DeleteArrayElementAtIndex(i);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

