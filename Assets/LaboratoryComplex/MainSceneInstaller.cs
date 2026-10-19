
using UnityEngine;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private Canvas _mainCanvas;

    public override void InstallBindings()
    {
        Container.Bind<CameraSwitcher>()
            .AsSingle()
            .WithArguments(_mainCamera);

        Container.Bind<Camera>()
            .WithId("MainCamera")
            .FromInstance(_mainCamera)
            .AsSingle();

        Container.Bind<Canvas>()
            .WithId("MainCanvas")
            .FromInstance(_mainCanvas)
            .AsSingle();
    }
}
