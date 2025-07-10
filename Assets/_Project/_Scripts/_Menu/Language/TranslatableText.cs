// TranslatableText.cs

using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

/// <summary>
/// Actualiza un TextMeshProUGUI usando el sistema de localización.
/// Se suscribe a OnLanguageChanged para refrescar en tiempo real.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatableText : MonoBehaviour
{
    [Tooltip("Clave del texto que se mostrará aquí.")]
    public TextKey textKey;

    [Header("Solo para preview en Editor")]
    [Tooltip("LanguageData de ejemplo para ver el texto en Editor.")]
    public LanguageData previewLanguageData;

    private TextMeshProUGUI _uiText;
    private TextKey _lastKey;

    private void Awake()
    {
        _uiText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText();
        }
        else
        {
            Debug.LogWarning($"[{nameof(TranslatableText)}] LanguageManager no está listo.");
        }
    }

    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
    }

    /// <summary>
    /// Crea el texto traducido según la clave actual.
    /// </summary>
    private void UpdateText()
    {
        _uiText.text = LanguageManager.Instance.GetText(textKey);
    }

#if UNITY_EDITOR
    /// <summary>
    /// En Editor: renombra el GameObject y muestra preview en el Inspector.
    /// </summary>
    private void OnValidate()
    {
        // Renombrar objeto al cambiar la clave
        if (_lastKey != textKey)
        {
            _lastKey = textKey;
            gameObject.name = textKey.ToString();
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
        }

        // Mostrar preview usando el LanguageData asignado
        if (previewLanguageData != null)
        {
            var entry = previewLanguageData.texts.FirstOrDefault(e => e.key == textKey);
            if (_uiText == null) _uiText = GetComponent<TextMeshProUGUI>();
            if (_uiText != null && _uiText.text != entry.value)
            {
                _uiText.text = entry.value;
                EditorUtility.SetDirty(_uiText);
            }
        }
    }
#endif
}





