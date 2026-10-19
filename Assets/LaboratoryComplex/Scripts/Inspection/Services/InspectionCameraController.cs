using UnityEngine;

public class InspectionCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _modelRoot;

    [Header("Camera")]
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _maxDistance = 100f;
    [SerializeField] private float _padding = 1.3f;

    [Header("Rotation")]
    [SerializeField] private float _minPitch = -80f;
    [SerializeField] private float _maxPitch = 80f;

    private Vector3 _target;
    private float _distance;
    private float _yaw;
    private float _pitch;

    private float _initialDistance;

    private void Start()
    {
        if (_modelRoot != null)
            FocusOn(_modelRoot);
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            _yaw += Input.GetAxis("Mouse X") * _rotationSpeed;
            _pitch -= Input.GetAxis("Mouse Y") * _rotationSpeed;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _distance = Mathf.Clamp(
            _distance - scroll * _zoomSpeed * _distance,
            _minDistance,
            _maxDistance);

        UpdateCameraPosition();
    }

    public void FocusOn(Transform model)
    {
        if (model == null)
            return;

        _modelRoot = model;

        if (!TryGetBounds(model, out Bounds bounds))
        {
            Debug.LogWarning($"Model '{model.name}' has no visible Renderers.", model);
            return;
        }

        _target = bounds.center;

        float verticalFov = _cameraTransform.GetComponent<Camera>() != null
            ? _cameraTransform.GetComponent<Camera>().fieldOfView
            : Camera.main.fieldOfView;

        Camera camera = _cameraTransform.GetComponent<Camera>();
        if (camera == null)
            camera = Camera.main;

        if (camera == null)
        {
            Debug.LogError("InspectionCameraController: Camera not found.", this);
            return;
        }

        verticalFov = camera.fieldOfView * Mathf.Deg2Rad;

        float horizontalFov = 2f * Mathf.Atan(
            Mathf.Tan(verticalFov / 2f) * camera.aspect);

        float verticalDistance = bounds.extents.y /
            Mathf.Tan(verticalFov / 2f);

        float horizontalDistance = bounds.extents.x /
            Mathf.Tan(horizontalFov / 2f);

        _initialDistance = Mathf.Max(verticalDistance, horizontalDistance)
            + bounds.extents.z;

        _distance = Mathf.Clamp(
            _initialDistance * _padding,
            _minDistance,
            _maxDistance);

        Vector3 direction = _cameraTransform.position - _target;
        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();
            _yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _pitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }

        UpdateCameraPosition();
    }

    public void ResetView()
    {
        if (_modelRoot != null)
            FocusOn(_modelRoot);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * (Vector3.back * _distance);

        _cameraTransform.position = _target + offset;
        _cameraTransform.LookAt(_target);
    }

    private static bool TryGetBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        bounds = default;

        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
                continue;

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return found;
    }
}