using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadSceneIfNotLoaded("_Project/_Scenes/_Scenarios/Meadow/Meadow_V0.0.0");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadSceneIfNotLoaded("_Project/_Scenes/_Scenarios/ProceduralMap/AutoPath_V0.1.1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadSceneIfNotLoaded("_Project/_Scenes/_Scenarios/Forest/Forest_V0.0.0");
        }
    }

    void LoadSceneIfNotLoaded(string sceneName)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
