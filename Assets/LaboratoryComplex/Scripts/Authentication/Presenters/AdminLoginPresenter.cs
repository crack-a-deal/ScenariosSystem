using System;
using UnityEngine;
using Zenject;


public class AdminLoginPresenter : IDisposable
{
    public event Action AuthorizationSuccess;
    public event Action BackClicked;

    private readonly LoginView _view;

    private readonly IValidateUsersService _validateUsersService;
    private readonly UserSession _userSession;

    public AdminLoginPresenter([Inject(Id = "Admin")] LoginView view, IValidateUsersService validateUsersService, UserSession userSession)
    {
        _view = view;
        _validateUsersService = validateUsersService;
        _userSession = userSession;

        _view.LoginClicked += OnLoginClicked;
        _view.BackClicked += OnBackClicked;
    }

    public void Dispose()
    {
        _view.LoginClicked -= OnLoginClicked;
        _view.BackClicked -= OnBackClicked;
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
        _view.ClearError();
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private async void OnLoginClicked()
    {
        AuthorizationSuccess?.Invoke();
        return;

        _view.ClearError();

        if (string.IsNullOrWhiteSpace(_view.Password))
        {
            _view.ShowError("Пожалуйста, введите пароль");
            _view.SetPasswordOutline(true);
            return;
        }

        string login = "admin";

        try
        {
            var result = await _validateUsersService.ValidateUserAsync(login, _view.Password);

            if (!result.Item1)
            {
                _view.ShowError("Неверный пароль");
                _view.SetPasswordOutline(true);

                return;
            }
            //_userSession.SignIn(result.Item2, UserRole.Teacher);
            AuthorizationSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error {ex.Message}");
        }
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}

