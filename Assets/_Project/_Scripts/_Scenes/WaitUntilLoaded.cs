using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class WaitUntilLoaded : MonoBehaviour
{
    [SerializeField] private TMP_Text _progressText;  // Arrastra aquí tu TextMeshProUGUI

    private void Start()
    {
        int targetIndex = SettingsManager.Instance.KnowYourLevel();
        StartCoroutine(LoadTargetSceneAsync(targetIndex));
    }

    private IEnumerator LoadTargetSceneAsync(int sceneIndex)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            int percent = Mathf.FloorToInt(progress * 100f);
            _progressText.text = percent + "%";
            yield return null;
        }

        // Ya está cargada al 90% (operación en segundo plano lista)
        _progressText.text = "100%";  // Opcional: garantizar 100%
        op.allowSceneActivation = true;
        yield return null;
    }
}


