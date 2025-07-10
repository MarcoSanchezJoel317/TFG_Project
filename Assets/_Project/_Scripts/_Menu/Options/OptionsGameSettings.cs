using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla el dropdown de selección de idioma en la sección de opciones.
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class OptionsGameSettings : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Dropdown que muestra los idiomas disponibles.")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Awake()
    {
        if (languageDropdown == null)
        {
            Debug.LogError("[OptionsGameSettings] Falta asignar languageDropdown.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        SetupLanguageDropdown();
    }

    private void OnDisable()
    {
        languageDropdown.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    /// Rellena el dropdown con los idiomas de LanguageManager
    /// y suscribe el evento para cambiar de idioma.
    /// </summary>
    private void SetupLanguageDropdown()
    {
        var manager = LanguageManager.Instance;
        languageDropdown.ClearOptions();

        // Añade cada idioma por su displayName
        var options = manager.AvailableLanguages
                             .Select(ld => ld.displayName)
                             .ToList();
        languageDropdown.AddOptions(options);

        // Ajusta el índice guardado
        languageDropdown.value = manager.GetCurrentLanguageIndex();
        languageDropdown.RefreshShownValue();

        // Cambia el idioma al seleccionar una opción
        languageDropdown.onValueChanged.AddListener(idx =>
        {
            manager.SetLanguage(idx);
        });
    }

    /// <summary>
    /// Ejemplo de método que abre una URL externa.
    /// </summary>
    public void OpenExternalLink()
    {
        Application.OpenURL("https://albaro.dev/");
    }
}

