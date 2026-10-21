using UnityEngine;
using Zenject;

public class GuardantLicenseInstaller : MonoInstaller
{
    [SerializeField, Min(1f)]
    private float _checkIntervalSeconds = 10f;

    [SerializeField, Min(0f)]
    private float _gracePeriodSeconds = 30f;

    [SerializeField]
    private string _configPath = "";

    public override void InstallBindings()
    {
        Container
            .BindInterfacesAndSelfTo<GuardantLicenseService>()
            .AsSingle()
            .WithArguments(
                _checkIntervalSeconds,
                _gracePeriodSeconds,
                _configPath);
    }
}