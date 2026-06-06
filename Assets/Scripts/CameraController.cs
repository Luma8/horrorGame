using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private float verticalClamp = 80f;

    [SerializeField] private GameObject spotLightPrefab;

    [Header("Head Bob")]
    [SerializeField] private float bobFrequencyWalk = 1.4f;
    [SerializeField] private float bobAmplitudeWalkY = 0.05f;
    [SerializeField] private float bobAmplitudeWalkX = 0.025f;
    [SerializeField] private float bobFrequencySprint = 2.2f;
    [SerializeField] private float bobAmplitudeSprintY = 0.1f;
    [SerializeField] private float bobAmplitudeSprintX = 0.05f;
    [SerializeField] private float bobSmoothing = 10f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private InputSystem_Actions _input;
    private float _xRotation;
    private float _yRotation;
    private float _bobTimer;
    private Vector3 _targetBobOffset;
    private Vector3 _originalLocalPos;

    private void Awake() => _input = new InputSystem_Actions();
    private void OnEnable() => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Start()
    {
        _originalLocalPos = transform.localPosition;
    }

    private void Update()
    {
        HandleLook();
        HandleBob();
    }

    private void HandleLook()
    {
        Vector2 lookInput = _input.Player.Look.ReadValue<Vector2>();

        _xRotation -= lookInput.y * sensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);

        _yRotation += lookInput.x * sensitivity;

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }

    private void HandleBob()
    {
        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        bool sprinting = _input.Player.Sprint.IsPressed();
        bool moving = moveInput.sqrMagnitude > 0.01f;

        if (moving)
        {
            float freq = sprinting ? bobFrequencySprint : bobFrequencyWalk;
            float ampY = sprinting ? bobAmplitudeSprintY : bobAmplitudeWalkY;
            float ampX = sprinting ? bobAmplitudeSprintX : bobAmplitudeWalkX;

            _bobTimer += Time.deltaTime * freq * Mathf.PI * 2f;

            _targetBobOffset = new Vector3(
                Mathf.Sin(_bobTimer * 0.5f) * ampX,
                Mathf.Sin(_bobTimer) * ampY,
                0f
            );
        }
        else
        {
            _targetBobOffset = Vector3.zero;
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _originalLocalPos + _targetBobOffset,
            Time.deltaTime * bobSmoothing
        );
    }
}
