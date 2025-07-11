using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la carga de escenas según su índice en Build Settings,
/// actualiza el nivel en SettingsManager y dispara el evento OnLevelChanged.
/// Recoge dinámicamente la lista de escenas configuradas en Build Settings.
/// </summary>
[DisallowMultipleComponent]
public class SceneLoader : MonoBehaviour
{
    // Array interno con todos los nombres de escena según Build Settings
    [SerializeField] private string[] _sceneNames;
    [SerializeField] private string _loadingSceneName = "WAIT";


    private void Awake()
    {
        
        // Obtener cuántas escenas hay en Build Settings
        int buildCount = SceneManager.sceneCountInBuildSettings;
        _sceneNames = new string[buildCount];

        // Rellenar nombres quitando la ruta y la extensión
        for (int i = 0; i < buildCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            _sceneNames[i] = Path.GetFileNameWithoutExtension(path);
        }
    }

    /// <summary>
    /// Carga la escena cuyo índice en Build Settings es sceneIndex.
    /// También ajusta PlayerPrefs y lanza el evento OnLevelChanged.
    /// </summary>
    /// <param name="sceneIndex">Índice de la escena (0 = Menú, 1 = Bosque, 2 = Nieve…)</param>
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Length)
        {
            Debug.LogError($"[SceneLoader] Índice de escena inválido: {sceneIndex}");
            return;
        }

        // Guardamos el nivel (desencadena OnLevelChanged)
        SettingsManager.Instance.SetYourLevel(sceneIndex);

        // Cargamos la escena por nombre
        SceneManager.LoadScene(_loadingSceneName);
    }

    /// <summary>
    /// Método pensado para el botón START en el Menú:
    /// si el nivel actual es 1 o 2, recarga esa; si sigue en 0, arranca en 1.
    /// Úsalo en onClick del botón como LoadStartGame().
    /// </summary>
    public void LoadStartGame()
    {
        int current = SettingsManager.Instance.KnowYourLevel();
        int target = (current == 1 || current == 2) ? current : 1;
        LoadScene(target);
    }

    public void LoadExit()
    {
#if UNITY_EDITOR
        // Si estamos en el Editor, salir del modo Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // En una build ejecutable, cerrar la aplicación
        Application.Quit();
#endif
    }

}


