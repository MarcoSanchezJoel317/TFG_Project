using UnityEngine;

public class CustomizeCursor : MonoBehaviour
{

    [SerializeField] GameObject cursor2;



    private void Awake()
    {
        if(SettingsManager.Instance.KnowYourLevel() == 2)
        {
            cursor2.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (SettingsManager.Instance.KnowYourLevel() == 2)
        {
            cursor2.SetActive(true);
        }
    }
}
