using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions _inputActions;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] private Transform _camera;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    private CharacterController _characterController;

    private float _verticalVelocity;
    private float _cameraPitch;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
    }

    private void Update()
    {
        Move();
        Look();
    }

    [SerializeField] private bool _canLook = true;

    public void SetLookEnabled(bool enabled)
    {
        _canLook = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void Move()
    {
        Vector2 input = _inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 direction = transform.right * input.x +
                            transform.forward * input.y;

        direction.Normalize();

        if (_characterController.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += _gravity * Time.deltaTime;

        Vector3 velocity = direction * _moveSpeed;
        velocity.y = _verticalVelocity;

        _characterController.Move(velocity * Time.deltaTime);
    }

    private void Look()
    {
        if (!_canLook)
            return;

        Vector2 input = _inputActions.Player.Look.ReadValue<Vector2>();

        float mouseX = input.x * _mouseSensitivity;
        float mouseY = input.y * _mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        _cameraPitch -= mouseY;
        _cameraPitch = Mathf.Clamp(_cameraPitch, _minPitch, _maxPitch);

        _camera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }
}