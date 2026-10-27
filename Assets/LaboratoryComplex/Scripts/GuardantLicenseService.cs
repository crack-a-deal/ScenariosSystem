using Protection;
using Protection.Core.Base;
using Protection.Core.Data;
using System;
using UnityEngine;
using Zenject;

public enum GuardantLicenseState
{
    NotChecked,
    Licensed,
    GracePeriod,
    Blocked
}

public class GuardantLicenseService :
    IInitializable,
    ITickable,
    IDisposable
{
    private readonly float _checkIntervalSeconds;
    private readonly float _gracePeriodSeconds;
    private readonly string _configPath;

    private ProtectionManager _protectionManager;

    private float _checkTimer;
    private float _graceRemainingSeconds;
    private bool _disposed;

    public GuardantLicenseState State { get; private set; }
        = GuardantLicenseState.NotChecked;

    public ProtectionResponse? LastResponse { get; private set; }

    public string LastKey { get; private set; }

    public string LastError { get; private set; }

    public float GraceRemainingSeconds =>
        Mathf.Max(0f, _graceRemainingSeconds);

    public bool IsLicensed =>
        State == GuardantLicenseState.Licensed;

    public bool CanUseProtectedFeatures =>
        State == GuardantLicenseState.Licensed ||
        State == GuardantLicenseState.GracePeriod;

    public event Action<GuardantLicenseState> StateChanged;

    public event Action<ProtectionResponse, string>
        ProtectionCheckCompleted;

    public event Action<string> ProtectionCheckFailed;

    public GuardantLicenseService(
        float checkIntervalSeconds = 10f,
        float gracePeriodSeconds = 30f,
        string configPath = "")
    {
        _checkIntervalSeconds = Mathf.Max(1f, checkIntervalSeconds);
        _gracePeriodSeconds = Mathf.Max(0f, gracePeriodSeconds);
        _configPath = configPath ?? string.Empty;
    }

    public void Initialize()
    {
        if (_disposed)
            return;

        CheckNow();

        _checkTimer = 0f;
    }

    public void Tick()
    {
        //if (_disposed)
        //    return;

        //float deltaTime = Time.unscaledDeltaTime;

        //if (State == GuardantLicenseState.GracePeriod)
        //{
        //    _graceRemainingSeconds -= deltaTime;

        //    if (_graceRemainingSeconds <= 0f)
        //    {
        //        _graceRemainingSeconds = 0f;
        //        SetState(GuardantLicenseState.Blocked);
        //    }
        //}

        //_checkTimer += deltaTime;

        //if (_checkTimer >= _checkIntervalSeconds)
        //{
        //    _checkTimer = 0f;
        //    CheckNow();
        //}
    }

    public void CheckNow()
    {
        if (_disposed)
            return;

        try
        {
            if (_protectionManager == null)
            {
                _protectionManager =
                    new ProtectionManager(_configPath);
            }

            var result = _protectionManager.CheckProtection();

            LastResponse = result.Item1;
            LastKey = result.Item2;
            LastError = null;

            if (result.Item1 == ProtectionResponse.Ok)
            {
                _graceRemainingSeconds = _gracePeriodSeconds;
                SetState(GuardantLicenseState.Licensed);
            }
            else
            {
                string message =
                    $"Guardant check failed: {result.Item1}";

                if (!string.IsNullOrWhiteSpace(result.Item2))
                    message += $". Key: {result.Item2}";

                Debug.Log(message);

                HandleFailedCheck(message);
            }

            ProtectionCheckCompleted?.Invoke(
                result.Item1,
                result.Item2);
        }
        catch (Exception exception)
        {
            LastResponse = null;
            LastKey = null;
            Debug.Log($"Guardant initialization/check error: {exception.Message}");
            string message =
                $"Guardant initialization/check error: {exception.Message}";

            HandleFailedCheck(message);
        }
    }

    private void HandleFailedCheck(string message)
    {
        LastError = message;
        ProtectionCheckFailed?.Invoke(message);

        if (State == GuardantLicenseState.NotChecked ||
            State == GuardantLicenseState.Licensed)
        {
            _graceRemainingSeconds = _gracePeriodSeconds;

            SetState(
                _graceRemainingSeconds > 0f
                    ? GuardantLicenseState.GracePeriod
                    : GuardantLicenseState.Blocked);
        }
        else if (State == GuardantLicenseState.GracePeriod &&
                 _graceRemainingSeconds <= 0f)
        {
            SetState(GuardantLicenseState.Blocked);
        }
    }

    private void SetState(GuardantLicenseState newState)
    {
        if (State == newState)
            return;

        State = newState;
        StateChanged?.Invoke(State);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _protectionManager?.Dispose();
        _protectionManager = null;

        StateChanged = null;
        ProtectionCheckCompleted = null;
        ProtectionCheckFailed = null;
    }
}