using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;

/// <summary>
/// Gestiona la música de fondo cargando el AudioClip adecuado vía Addressables
/// según el nivel actual (0=Menú, 1=Bosque, 2=Nieve), evitando recargas redundantes
/// y liberando recursos correctamente.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CustomizeMusic : MonoBehaviour
{
    [Header("Claves Addressable para música por nivel")]
    [Tooltip("Nivel 0: Música del Menú")]
    [SerializeField] private string _addressMenuLevel0 = "MusicMenuLevel0";
    [Tooltip("Nivel 1: Música del Bosque")]
    [SerializeField] private string _addressForestLevel1 = "MusicForestLevel1";
    [Tooltip("Nivel 2: Música de la Nieve")]
    [SerializeField] private string _addressBlizzardLevel2 = "MusicBlizzardLevel2";

    private AudioSource _audioSource;
    private AsyncOperationHandle<AudioClip>? _currentHandle;
    private string _currentAddress;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento para recargar al cambiar de nivel
        SettingsManager.OnLevelChanged += ReloadMusicAsync;
        // Cargamos al inicio según el nivel actual
        ReloadMusicAsync(SettingsManager.Instance.KnowYourLevel());
    }

    private void OnDisable()
    {
        SettingsManager.OnLevelChanged -= ReloadMusicAsync;
        ReleaseCurrentClip();
        _cts?.Cancel();
    }

    /// <summary>
    /// Carga y reproduce de forma asíncrona la música correspondiente al nivel.
    /// </summary>
    /// <param name="level">0=Menú, 1=Bosque, 2=Nieve</param>
    private async void ReloadMusicAsync(int level)
    {
        // Determinar la dirección según el nivel
        string targetAddress = level switch
        {
            0 => _addressMenuLevel0,
            1 => _addressForestLevel1,
            2 => _addressBlizzardLevel2,
            3 => _addressMenuLevel0,
            4 => _addressMenuLevel0,
            _ => _addressMenuLevel0 // fallback al menú
        };

        // Si ya estamos reproduciendo este clip, salir
        if (targetAddress == _currentAddress)
            return;

        _currentAddress = targetAddress;
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Parar y liberar el clip anterior
        _audioSource.Stop();
        ReleaseCurrentClip();

        try
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(targetAddress);
            _currentHandle = handle;
            await handle.Task;

            if (token.IsCancellationRequested)
                return;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _audioSource.clip = handle.Result;
                _audioSource.Play();
            }
            else
            {
                Debug.LogError($"[CustomizeMusic] Error cargando audio en '{targetAddress}'.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CustomizeMusic] Excepción al cargar audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Libera el handle actual si es válido.
    /// </summary>
    private void ReleaseCurrentClip()
    {
        if (_currentHandle.HasValue && _currentHandle.Value.IsValid())
        {
            Addressables.Release(_currentHandle.Value);
            _currentHandle = null;
        }
    }
}


