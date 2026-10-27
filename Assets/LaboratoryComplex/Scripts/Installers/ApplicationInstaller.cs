using Zenject;

public class ApplicationInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<MyApplication>().AsSingle();

        Container.Bind<IApplication>()
            .FromResolveGetter<MyApplication>(application => application)
            .AsSingle();

        Container.Bind<BootstrapState>().AsSingle();
        Container.Bind<RoleSelectionState>().AsSingle();
        Container.Bind<TeacherAuthorizationState>().AsSingle();
        Container.Bind<StudentAuthorizationState>().AsSingle();
        Container.Bind<TeacherMainState>().AsSingle();
        Container.Bind<StudentWaitingState>().AsSingle();
        Container.Bind<StudentMainState>().AsSingle();
        Container.Bind<TestState>().AsSingle();
        Container.Bind<InspectionModelState>().AsSingle();
        Container.Bind<ScenarioState>().AsSingle();

        Container.BindInterfacesTo<ApplicationEntryPoint>()
            .AsSingle()
            .NonLazy();

        Container.Bind<UserSession>().AsSingle();
    }
}
