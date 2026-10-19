using TMPro;
using UnityEngine;

[System.Serializable]
public class ShowMessageAction : ScenarioAction
{
    [TextArea(2, 5)]
    [SerializeField] private string _message;

    [SerializeField] private GameObject _messageBox;
    [SerializeField] private TMP_Text _messageLabel;

    public override void Execute(ScenarioContext context)
    {
        _messageBox.SetActive(true);
        _messageLabel.text = _message;
    }
}

[System.Serializable]
public class HideMessageAction : ScenarioAction
{
    [SerializeField] private GameObject _messageBox;
    [SerializeField] private TMP_Text _messageLabel;

    public override void Execute(ScenarioContext context)
    {
        _messageBox.SetActive(false);
        _messageLabel.text = string.Empty;
    }
}