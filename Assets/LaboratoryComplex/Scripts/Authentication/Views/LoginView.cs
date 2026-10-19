using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    public event Action LoginClicked;
    public event Action BackClicked;

    [SerializeField] private TMP_InputField _loginInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _loginButton;

    [SerializeField] private Button _backButton;

    [SerializeField] private Outline loginOutline;
    [SerializeField] private Outline passwordOutline;

    [SerializeField] private GameObject _errorBlock;
    [SerializeField] private TMP_Text _errorText;


    public string Login => _loginInput.text;
    public string Password => _passwordInput.text;

    private void OnEnable()
    {
        _loginButton.onClick.AddListener(OnLoginClicked);
        _backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDisable()
    {
        _loginButton.onClick.RemoveListener(OnLoginClicked);
        _backButton.onClick.RemoveListener(OnBackClicked);
    }

    public void ShowError(string message)
    {
        _errorBlock.SetActive(true);
        _errorText.text = message;
    }

    public void SetLoginOutline(bool value)
    {
        if (_loginInput != null)
        {
            loginOutline.enabled = value;
        }
    }

    public void SetPasswordOutline(bool value)
    {
        if (_passwordInput != null)
        {
            passwordOutline.enabled = value;
        }
    }

    public void ClearError()
    {
        _errorBlock.SetActive(false);
        SetLoginOutline(false);
        SetPasswordOutline(false);

        _errorText.text = string.Empty;
    }

    private void OnLoginClicked()
    {
        LoginClicked?.Invoke();
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}
