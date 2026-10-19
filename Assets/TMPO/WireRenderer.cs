
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WireRenderer : MonoBehaviour
{
    [Header("Sling")]
    [SerializeField] private Transform _grabPoint;
    [SerializeField] private Transform[] _connectionPoints = new Transform[4];

    [Header("Appearance")]
    [SerializeField, Min(0.001f)] private float _width = 0.008f;
    [SerializeField, Min(0f)] private float _sag = 0.1f;
    [SerializeField, Range(2, 20)] private int _segments = 4;

    private LineRenderer[] _lineRenderers;
    private Vector3[][] _positions;

    private void Awake()
    {
        int count = _connectionPoints.Length;

        _lineRenderers = new LineRenderer[count];
        _positions = new Vector3[count][];

        for (int i = 0; i < count; i++)
        {
            GameObject lineObject = new GameObject($"Sling_{i + 1}");
            lineObject.transform.SetParent(transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();

            line.useWorldSpace = true;
            line.startWidth = _width;
            line.endWidth = _width;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;

            LineRenderer source = GetComponent<LineRenderer>();
            line.sharedMaterial = source.sharedMaterial;
            line.startColor = source.startColor;
            line.endColor = source.endColor;
            line.widthMultiplier = source.widthMultiplier;

            _lineRenderers[i] = line;
            _positions[i] = new Vector3[_segments + 1];

            line.positionCount = _segments + 1;
        }

        GetComponent<LineRenderer>().enabled = false;
    }

    private void LateUpdate()
    {
        if (_grabPoint == null)
            return;

        for (int i = 0; i < _connectionPoints.Length; i++)
        {
            Transform connection = _connectionPoints[i];

            if (connection == null)
            {
                _lineRenderers[i].enabled = false;
                continue;
            }

            _lineRenderers[i].enabled = true;

            UpdateSling(
                _lineRenderers[i],
                _positions[i],
                _grabPoint.position,
                connection.position);
        }
    }

    private void UpdateSling(
        LineRenderer line,
        Vector3[] positions,
        Vector3 start,
        Vector3 end)
    {
        for (int i = 0; i <= _segments; i++)
        {
            float t = (float)i / _segments;

            Vector3 position = Vector3.Lerp(start, end, t);

            position.y -= 4f * _sag * t * (1f - t);

            positions[i] = position;
        }

        line.SetPositions(positions);
    }
}
