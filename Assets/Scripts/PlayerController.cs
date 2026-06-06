using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;

    private CharacterController _controller;
    private InputSystem_Actions _input;
    private Vector3 _velocity;
    private bool _isCrouching;

    public InputSystem_Actions Input => _input;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => _input.Player.Enable();
    private void OnDisable() => _input.Player.Disable();

    private void Update()
    {
        HandleMovement();
        HandleCrouch();
    }

    private void HandleMovement()
    {
        bool grounded = _controller.isGrounded;

        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        Vector2 moveInput = _input.Player.Move.ReadValue<Vector2>();
        bool sprinting = _input.Player.Sprint.IsPressed() && !_isCrouching;
        float speed = sprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        _controller.Move(move * speed * Time.deltaTime);

        if (_input.Player.Jump.WasPressedThisFrame() && grounded && !_isCrouching)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (!_input.Player.Crouch.WasPressedThisFrame()) return;

        _isCrouching = !_isCrouching;
        _controller.height = _isCrouching ? crouchHeight : standHeight;
        _controller.center = new Vector3(0f, _controller.height * 0.5f, 0f);
    }
}
