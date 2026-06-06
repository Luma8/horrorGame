using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private float verticalClamp = 80f;

    [SerializeField] private GameObject spotLightPrefab;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private InputSystem_Actions _input;
    private float _xRotation;
    private float _yRotation;

    private void Awake() => _input = new InputSystem_Actions();
    private void OnEnable() => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();


    private void Update()
    {
        Vector2 lookInput = _input.Player.Look.ReadValue<Vector2>();

        _xRotation -= lookInput.y * sensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);

        _yRotation += lookInput.x * sensitivity;

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }
}
