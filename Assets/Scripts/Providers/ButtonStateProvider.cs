using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonStateProvider : MonoBehaviour, IBooleanStateProvider
{
    [SerializeField] private ScenarioEventBus _eventBus;

    private Button _button;
    private bool _value;
    public bool Value => _value;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void SetValue(bool value)
    {
        _value = value;
    }

    private void OnClick()
    {
        SetValue(true);
        _eventBus.Publish(new ScenarioEvent(ScenarioEventType.ButtonPressed,_button.gameObject));
    }
}