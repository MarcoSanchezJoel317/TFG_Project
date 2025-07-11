using UnityEngine;

public class InfoManager : MonoBehaviour
{        
    public GameObject objectToActivate;
    private static InfoManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Se mantiene entre escenas
        }
        else
        {
            Destroy(gameObject); // Elimina duplicados
        }
    }

        void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && objectToActivate != null && !objectToActivate.activeInHierarchy)        
            objectToActivate.SetActive(true);        
        else if (Input.GetKeyDown(KeyCode.Escape) && objectToActivate != null && objectToActivate.activeInHierarchy)
            objectToActivate.SetActive(false);
    }
}
