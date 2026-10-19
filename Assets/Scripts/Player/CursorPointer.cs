using System;
using UnityEngine;

public class CursorPointer : MonoBehaviour
{
    public event Action<GameObject> ObjectChanged;

    [SerializeField] private Camera _camera;
    [SerializeField] private float _maxDistance = 5f;
    [SerializeField] private LayerMask _layerMask = ~0;

    public GameObject CurrentObject { get; private set; }

    private void Update()
    {
        UpdatePointer();
    }

    private void UpdatePointer()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        GameObject target = null;

        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _layerMask))
        {
            target = hit.collider.gameObject;
        }

        if (target == CurrentObject)
        {
            return;
        }

        CurrentObject = target;
        ObjectChanged?.Invoke(CurrentObject);
    }
}