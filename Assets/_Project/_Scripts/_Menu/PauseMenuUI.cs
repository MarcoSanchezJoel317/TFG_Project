using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // ← necesario

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    //private InputSystem_Actions inputActions;

    public TextMeshProUGUI startText; // Arrastra el Text/TextMeshPro UI.
    public TextMeshProUGUI optionsText; // Arrastra el Text/TextMeshPro UI.
    public TextMeshProUGUI exitText; // Arrastra el Text/TextMeshPro UI.

    public TMP_Dropdown dropdown;

    private void Awake()
    {
        //inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        //inputActions.UI.Pause.Enable();
        //inputActions.UI.Pause.performed += OnPause;
        UpdateUITexts();
        dropdown.value = LanguageManager.Instance.GetCurrentLanguageIndex();
    }

    private void OnDisable()
    {
        //inputActions.UI.Pause.performed -= OnPause;
        //inputActions.UI.Pause.Disable();
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        canvas.SetActive(!canvas.activeSelf);
    }

    public void UpdateUITexts()
    {
        //startText.text = LanguageManager.Instance.GetText(TextKey.MM_START);
        //optionsText.text = LanguageManager.Instance.GetText("options");
        //exitText.text = LanguageManager.Instance.GetText("exit");
    }
}

