using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class StudentLoginPresenter
{
    public event Action AuthorizationSuccess;
    public event Action BackClicked;

    private readonly LoginView _view;
    private readonly TcpClientProvider _clientProvider;
    private readonly UserSession _userSession;

    private bool _isAuthenticating;
    private CancellationTokenSource _lifetimeCts;

    public StudentLoginPresenter([Inject(Id = "Student")] LoginView view, TcpClientProvider clientProvider, UserSession userSession)
    {
        _view = view;
        _clientProvider = clientProvider;
        _userSession = userSession;
        _view.LoginClicked += OnLoginClicked;
        _view.BackClicked += OnBackClicked;
    }

    public void Dispose()
    {
        _view.LoginClicked -= OnLoginClicked;
        _view.BackClicked -= OnBackClicked;

        _lifetimeCts?.Cancel();
        _lifetimeCts?.Dispose();
        _lifetimeCts = null;
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
        if (_isAuthenticating)
            return;

        _view.ClearError();

        string login = _view.Login?.Trim();
        string password = _view.Password;

        if (string.IsNullOrWhiteSpace(login))
        {
            _view.ShowError("Введите логин");
            _view.SetLoginOutline(true);
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _view.ShowError("Введите пароль");
            _view.SetPasswordOutline(true);
            return;
        }

        _isAuthenticating = true;

        using var cts = new CancellationTokenSource();
        _lifetimeCts = cts;

        try
        {
            var request = new AuthenticationRequest
            {
                Login = login,
                Password = password
            };

            AuthenticationResponse response = await _clientProvider.Instance.SendRequestAsync<AuthenticationResponse>(request, cts.Token);

            cts.Token.ThrowIfCancellationRequested();

            if (!_view.gameObject.activeInHierarchy)
                return;

            if (!response.Success)
            {
                _view.ShowError(response.ErrorMessage ?? "Неверный логин или пароль");

                _view.SetLoginOutline(true);
                _view.SetPasswordOutline(true);
                return;
            }

            _userSession.SignIn(response.AccountId, response.ProfileId, UserRole.Student);

            AuthorizationSuccess?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // Экран закрыт или операция отменена.
        }
        catch (Exception ex)
        {
            Debug.LogError($"Authentication failed: {ex}");

            if (_view.gameObject.activeInHierarchy)
                _view.ShowError("Не удалось связаться с сервером");
        }
        finally
        {
            if (ReferenceEquals(_lifetimeCts, cts))
                _lifetimeCts = null;

            _isAuthenticating = false;
        }
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}

