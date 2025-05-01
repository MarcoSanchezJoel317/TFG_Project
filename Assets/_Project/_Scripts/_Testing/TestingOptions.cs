using UnityEngine;

public class TestingOptions : MonoBehaviour
{

    public GameObject newCanvas;
    public GameObject oldCanvas;

    public void ReturnOptions()
    {
        newCanvas.SetActive(false);
        oldCanvas.SetActive(true);
    }

    public void Options()
    {
        newCanvas.SetActive(true);
        oldCanvas.SetActive(false);
    }



    public void TestingSound()
    {
        PlayerPrefs.SetFloat("SoundVolume", 0.5f);
    }
}
