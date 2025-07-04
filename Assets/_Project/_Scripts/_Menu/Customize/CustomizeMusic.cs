using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CustomizeMusic : MonoBehaviour
{
    [SerializeField] private string _addressNivel1 = "MusicLevel1";
    [SerializeField] private string _addressNivel2 = "MusicLevel2";

    private AudioSource _audioSource;
    private AsyncOperationHandle<AudioClip> _cargaAudio;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        CargarMusicaSegunNivel();
    }

    private void OnEnable()
    {
        CargarMusicaSegunNivel();
    }

    private void OnDestroy()
    {
        // Libera recursos cuando el objeto se destruya
        if (_cargaAudio.IsValid())
        {
            Addressables.Release(_cargaAudio);
        }
    }

    private async void CargarMusicaSegunNivel()
    {
        // Detiene música actual
        _audioSource.Stop();

        // Libera el clip anterior si existe
        if (_cargaAudio.IsValid())
        {
            Addressables.Release(_cargaAudio);
        }
        Debug.Log(SettingsManager.Instance.KnowYourLevel());


        // Determina qué audio cargar
        string address = SettingsManager.Instance.KnowYourLevel() == 1
            ? _addressNivel1
            : _addressNivel2;

        // Carga el clip de forma asíncrona
        _cargaAudio = Addressables.LoadAssetAsync<AudioClip>(address);
        await _cargaAudio.Task;

        if (_cargaAudio.Status == AsyncOperationStatus.Succeeded)
        {
            _audioSource.clip = _cargaAudio.Result;
            _audioSource.Play();
        }
        else
        {
            Debug.LogError("Error cargando audio: " + address);
        }
    }
}
