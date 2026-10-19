using UnityEngine;
using Zenject;

public class InspectionInstaller : MonoInstaller
{
    [SerializeField] private InspectionPartRegistry partRegistry;
    [SerializeField] private InspectionSelectionController selectionController;
    [SerializeField] private InspectionCameraController cameraController;
    [SerializeField] private InspectionView view;


    public override void InstallBindings()
    {
        Container.Bind<InspectionPartRegistry>()
            .FromInstance(partRegistry)
            .AsSingle();

        Container.Bind<InspectionSelectionController>()
            .FromInstance(selectionController)
            .AsSingle();

        Container.Bind<InspectionCameraController>()
            .FromInstance(cameraController)
            .AsSingle();

        Container.Bind<InspectionView>()
            .FromInstance(view)
            .AsSingle();

        Container.Bind<InspectionPresenter>()
            .AsSingle();
    }
}