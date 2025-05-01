using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionDisplay : MonoBehaviour
{
    public TextMeshProUGUI versionText;

    void Start()
    {
        versionText.text = "v " + Application.version;
    }
}
