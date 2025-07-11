using UnityEngine;
using UnityEngine.InputSystem;

public class RechargeSprint : MonoBehaviour
{
    [SerializeField] InputActionReference _interactAction;
    private bool _isInRechargeZone = false;
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] private SheepMovementController _sheepController;


    private void OnEnable()
    {
        _interactAction.action.performed += OnInteractPerformed;
        _particleSystem.Stop();
    }
    private void OnDisable()
    {
        _interactAction.action.performed -= OnInteractPerformed;
        _particleSystem.Stop();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (_isInRechargeZone)
        {
            Recharge();
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("The Will"))
        {
            _isInRechargeZone = true;
            _particleSystem.Play();

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isInRechargeZone = false;
            _particleSystem.Stop();

        }
    }
    private void Recharge()
    {
        _sheepController.ReloadEnergy();
    }




}
