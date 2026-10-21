using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerRowView : MonoBehaviour
{
    public enum ComputerRowButton
    {
        Connect,
        Disconnect
    }

    [SerializeField] private Image connectionIcon;

    [SerializeField] private TMP_Text ipAddressText;
    [SerializeField] private TMP_Text hostNameText;

    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconectButton;

    public event Action<ComputerRowButton> ButtonClicked;

    private void Awake()
    {
        connectButton.onClick.AddListener(
            () => ButtonClicked?.Invoke(ComputerRowButton.Connect));

        disconectButton.onClick.AddListener(
            () => ButtonClicked?.Invoke(ComputerRowButton.Disconnect));
    }

    public void SetIpAddress(string value)
    {
        ipAddressText.text = value;
    }

    public void SetHostName(string value)
    {
        hostNameText.text = value;
    }

    public void SetConnectionIcon(Sprite sprite)
    {
        connectionIcon.sprite = sprite;
    }

    public void ButtonActive(bool isActive)
    {
        connectButton.gameObject.SetActive(isActive);
    }
}
