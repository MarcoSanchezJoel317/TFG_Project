using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEditor.Timeline.Actions;

public class MenuManager : MonoBehaviour
{
    [Header("Fade Settings")]
    public SpriteRenderer fadeOverlay; // Sprite delante de la cámara
    public float fadeDuration = 0.5f;


    [SerializeField] private GameObject _fatherMenu;
    [SerializeField] private GameObject _childrenMenu;


    [SerializeField] private GameObject _currentMenu;

    [SerializeField] private InputActionReference _returnAction;




    private void Awake()
    {
        _returnAction.action.performed += _ => ReturnMenuPast();

        if (fadeOverlay != null)
        {
            // Configuración inicial transparente
            Color c = fadeOverlay.color;
            c.a = 0;
            fadeOverlay.color = c;
        }
    }

    void OnEnable()
    {
        _returnAction.action.Enable();
    }

    void OnDisable()
    {
        _returnAction.action.Disable();
    }

    public void ReturnMenuPast()
    {
        if(_currentMenu == _fatherMenu)
        {
            return;
        }
        else if(_currentMenu == _childrenMenu)
        {
            SwitchMenu(_fatherMenu);
        }
        else
        {
            SwitchMenu(_childrenMenu);
        }
    }


    public void SwitchMenu(GameObject nextMenu)
    {
        if (_currentMenu != nextMenu)
        {
            StartCoroutine(TransitionMenus(nextMenu));
        }
    }


    private IEnumerator TransitionMenus(GameObject nextMenu)
    {
        // Fade In (oscurecer)
        yield return StartCoroutine(FadeScreen(0, 1));

        // Cambiar menús
        if (_currentMenu != null) _currentMenu.SetActive(false);
        nextMenu.SetActive(true);
        _currentMenu = nextMenu;

        // Fade Out (aclarar)
        yield return StartCoroutine(FadeScreen(1, 0));
    }

    private IEnumerator FadeScreen(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeOverlay.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeOverlay.color = color;
    }

}