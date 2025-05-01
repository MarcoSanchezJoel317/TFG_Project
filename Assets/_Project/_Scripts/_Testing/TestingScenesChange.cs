using UnityEngine;
using UnityEngine.SceneManagement;

public class TestingScenesChange : MonoBehaviour
{
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
