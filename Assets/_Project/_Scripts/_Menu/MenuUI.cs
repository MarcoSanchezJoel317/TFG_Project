// MenuUI.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public List<TranslatableText> translatableTexts; // Lista de textos traducibles

    //public TextMeshProUGUI startText; // Arrastra el Text/TextMeshPro UI.
    //public TextMeshProUGUI optionsText; // Arrastra el Text/TextMeshPro UI.
    //public TextMeshProUGUI exitText; // Arrastra el Text/TextMeshPro UI.

    public TMP_Dropdown dropdown;

    private void Start()
    {
        // Llenar dropdown con nombres de los idiomas
        dropdown.options.Clear();
        foreach (LanguageData lang in LanguageManager.Instance.languages)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(lang.displayName));
        }

        // Seleccionar el idioma guardado en el dropdown
        dropdown.value = LanguageManager.Instance.GetCurrentLanguageIndex();
        dropdown.RefreshShownValue();

        // Escuchar cambios en el dropdown
        dropdown.onValueChanged.AddListener(ChangeLanguage);

        // Actualizar textos al inicio
        UpdateUITexts();
    }


    public void ChangeLanguage(int index)
    {
        LanguageManager.Instance.SetLanguage(index);
        UpdateUITexts(); // Actualiza toda la UI.
    }


    public void UpdateUITexts()
    {
        foreach (TranslatableText entry in translatableTexts)
        {
            entry.textElement.text = LanguageManager.Instance.GetText(entry.textKey);
        }
    }

}