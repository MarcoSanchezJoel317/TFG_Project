using UnityEngine;

public class TestingLoading : MonoBehaviour
{

    public AudioSource m_AudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        m_AudioSource.volume = PlayerPrefs.GetFloat("SoundVolume");

    }
}
