using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla el dropdown de selección de idioma en la sección de opciones.
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class OptionsGameSettings : MonoBehaviour
{
    [Tooltip("Dropdown de idiomas en tu Canvas.")]
    public TMP_Dropdown languageDropdown;

    private void Awake()
    {
        // Inicializamos el dropdown tan pronto como se active este GameObject
        SetupLanguageDropdown();
    }

    private void OnEnable()
    {
        // Nos aseguramos de refrescar en caso de reactivar el objeto
        SetupLanguageDropdown();
    }

    /// <summary>
    /// Rellena y configura el dropdown con los idiomas disponibles.
    /// </summary>
    private void SetupLanguageDropdown()
    {
        // 1. Limpiamos opciones previas
        languageDropdown.options.Clear();

        // 2. Añadimos cada idioma usando AvailableLanguages
        //    AvailableLanguages es IReadOnlyList<LanguageData>
        foreach (var langData in LanguageManager.Instance.AvailableLanguages)
        {
            languageDropdown.options.Add(
                new TMP_Dropdown.OptionData(langData.displayName)
            );
        }

        // 3. Seleccionamos el índice guardado
        int current = LanguageManager.Instance.GetCurrentLanguageIndex();
        languageDropdown.value = current;
        languageDropdown.RefreshShownValue();

        // 4. Nos suscribimos a cambios para actualizar el idioma
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(idx =>
            LanguageManager.Instance.SetLanguage(idx)
        );
    }

    /// <summary>
    /// Método de ejemplo para abrir una URL externa.
    /// </summary>
    public void EnlaceTemporal()
    {
        Application.OpenURL("https://albaro.dev/");
    }
}
