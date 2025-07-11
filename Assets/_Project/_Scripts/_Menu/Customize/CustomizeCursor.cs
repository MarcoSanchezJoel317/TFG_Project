using UnityEngine;

/// <summary>
/// Activa o desactiva un cursor alternativo según el nivel actual.
/// Se suscribe al evento OnLevelChanged para actualizarse en tiempo real.
/// </summary>
public class CustomizeCursor : MonoBehaviour
{
    [Header("Cursor alternativo")]
    [Tooltip("GameObject que representa el cursor para el nivel 2 (Nieve).")]
    [SerializeField] private GameObject _cursorBlizzard;

    /// <summary>
    /// Cachea el estado inicial y suscribe al evento de cambio de nivel.
    /// </summary>
    private void OnEnable()
    {
        UpdateCursor(SettingsManager.Instance.KnowYourLevel());
        //SettingsManager.OnLevelChanged += UpdateCursor;
    }

    /// <summary>
    /// Desuscribe del evento cuando este GameObject se desactiva.
    /// </summary>
    private void OnDisable()
    {
        //SettingsManager.OnLevelChanged -= UpdateCursor;
    }

    /// <summary>
    /// Activa el cursor adecuado según el nivel:
    /// sólo muestra _cursorSnow si el nivel es 2; en caso contrario lo oculta.
    /// </summary>
    /// <param name="level">Nivel actual del juego.</param>
    private void UpdateCursor(int level)
    {
        if (_cursorBlizzard != null)
        {
            _cursorBlizzard.SetActive(level == 2);
        }
        else
        {
            Debug.LogWarning("[CustomizeCursor] Falta asignar _cursorSnow en el Inspector.");
        }
    }
}
