using UnityEngine;
using UnityEngine.InputSystem;

public class Raton : MonoBehaviour
{
    [SerializeField] private InputActionReference _testInputAction;

    void OnEnable()
    {
        if (_testInputAction != null && _testInputAction.action != null)
        {
            _testInputAction.action.Enable();
            Debug.Log("MouseDebugTest: Test Input Action Habilitada.");
        }
    }

    void OnDisable()
    {
        if (_testInputAction != null && _testInputAction.action != null)
        {
            _testInputAction.action.Disable();
        }
    }

    void Update()
    {
        if (_testInputAction != null && _testInputAction.action != null)
        {
            Vector2 mouseDelta = _testInputAction.action.ReadValue<Vector2>();
            if (mouseDelta.magnitude > 0.001f) // Solo log si hay movimiento significativo
            {
                Debug.Log("MouseDebugTest: Mouse Delta: " + mouseDelta);
            }
        }
    }
}
