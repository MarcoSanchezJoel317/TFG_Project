using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[RequireComponent(typeof(TextMeshProUGUI))]
public class TranslatableText : MonoBehaviour
{
    [Tooltip("Clave de texto que quieres usar aquí")]
    public TextKey textKey;

    [Header("Sólo para preview en Editor: arrastra aquí tu LanguageData en Inglés")]
    public LanguageData previewLanguageData;

    private TextMeshProUGUI _uiText;

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
            Debug.LogWarning($"[{nameof(TranslatableText)}] LanguageManager no está listo todavía.");
        }
    }

    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText()
    {
        _uiText.text = LanguageManager.Instance.GetText(textKey);
    }

#if UNITY_EDITOR
    // Guarda la última clave para detectar cambios
    private TextKey _lastKey;

    private void OnValidate()
    {
        // 1) Renombrar el GameObject si cambió la clave
        if (_lastKey != textKey)
        {
            _lastKey = textKey;
            this.gameObject.name = textKey.ToString();
            // Marca dirty para que Unity guarde la escena/prefab
            if (!Application.isPlaying)
                EditorUtility.SetDirty(this);
        }

        // 2) Si hay LanguageData de preview, actualiza el texto al valor en inglés
        if (previewLanguageData != null)
        {
            // Busca la entrada en el array
            var entry = previewLanguageData.texts
                .FirstOrDefault(e => e.key == textKey);
            string englishValue = entry.value;

            // Asigna al TMP
            if (_uiText == null)
                _uiText = GetComponent<TextMeshProUGUI>();

            if (_uiText != null && _uiText.text != englishValue)
            {
                _uiText.text = englishValue;
                EditorUtility.SetDirty(_uiText);
            }
        }
    }
#endif
}




