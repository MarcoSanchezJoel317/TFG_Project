// OptionsManager.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsGameSettings : MonoBehaviour
{
    [Tooltip("Dropdown de idiomas en tu Canvas.")]
    public TMP_Dropdown languageDropdown;


    private void Awake()
    {
        SetLanguageDropdown();
    }
    private void OnEnable()
    {
        SetLanguageDropdown();
    }

    private void SetLanguageDropdown()
    {
        // 1. Rellenar opciones con los displayName de cada ScriptableObject
        languageDropdown.options.Clear();
        foreach (var lang in LanguageManager.Instance.languages)
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(lang.displayName));

        // 2. Seleccionar el valor guardado
        languageDropdown.value = LanguageManager.Instance.GetCurrentLanguageIndex();
        languageDropdown.RefreshShownValue();

        // 3. Suscribir el cambio
        languageDropdown.onValueChanged.AddListener(idx =>
            LanguageManager.Instance.SetLanguage(idx)
        );
    }


    public void EnlaceTemporal()
    {
        Application.OpenURL("https://albaro.dev/");
    }

}
