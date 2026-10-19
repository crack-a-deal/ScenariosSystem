using UnityEngine;

public class InspectionPart : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private InspectionPartData _data;
    [SerializeField] private Transform _inspectionTarget;
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private Color _highlightColor = Color.yellow;

    private MaterialPropertyBlock _propertyBlock;
    private Color[] _originalEmissionColors;
    private bool _isHighlighted;
    private bool _isVisible = true;

    public InspectionPartData Data => _data;
    public Transform InspectionTarget => _inspectionTarget != null ? _inspectionTarget : transform;
    public bool IsVisible => _isVisible;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();

        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<Renderer>(true);

        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        _originalEmissionColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            Material material = _renderers[i] != null ? _renderers[i].sharedMaterial : null;
            _originalEmissionColors[i] = material != null && material.HasProperty(EmissionColorId) ? material.GetColor(EmissionColorId) : Color.black;
        }
    }

    public void SetHighlight(bool enabled)
    {
        if (_isHighlighted == enabled)
            return;

        _isHighlighted = enabled;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer targetRenderer = _renderers[i];

            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(_propertyBlock);
            Color color = enabled ? _highlightColor : _originalEmissionColors[i];
            _propertyBlock.SetColor(EmissionColorId, color);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }

    public void SetVisible(bool visible)
    {
        if (_isVisible == visible)
            return;

        _isVisible = visible;

        if (!visible)
            SetHighlight(false);

        gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        if (_isHighlighted)
            SetHighlight(false);
    }
}
