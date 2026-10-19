using Zenject;

public class PresentersInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Application

        Container.Bind<MainPresenter>().AsSingle().NonLazy();
        Container.Bind<ExitPresenter>().AsSingle();

        // Authentication

        Container.Bind<RoleSelectionPresenter>().AsSingle();
        Container.Bind<AdminLoginPresenter>().AsSingle();
        Container.Bind<StudentLoginPresenter>().AsSingle();
        Container.Bind<StudentWaitingPresenter>().AsSingle();

        Container.Bind<StudentLearningSectionsPresenter>().AsSingle();
        Container.Bind<TeacherLearningSectionsPresenter>().AsSingle();

        // Administration

        Container.Bind<StudentFormPresenter>().AsSingle();
        Container.Bind<GroupFormPresenter>().AsSingle();

        Container.Bind<AdministrationPanelPresenter>().AsSingle();
        Container.Bind<AdminTablesPresenter>().AsSingle();
        Container.Bind<ComputersTablePresenter>().AsSingle();
        Container.Bind<StudentsTablePresenter>().AsSingle();
        Container.Bind<GroupsTablePresenter>().AsSingle();
        Container.Bind<TestsTablePresenter>().AsSingle();


        // Student

        Container.Bind<ModulesTablePresenter>().AsSingle();

        Container.Bind<StudentTestModulesTablePresenter>().AsSingle();
        Container.Bind<AdminTestModulesTablePresenter>().AsSingle();


        // Other
        //Container.Bind<TestSessionPresenterBase>().AsSingle();

        Container.Bind<AdminTestSessionPresenter>().AsSingle();
        Container.Bind<StudentTestSessionPresenter>().AsSingle();

        Container.Bind<TestEditorPanelPresenter>().AsSingle();
        Container.Bind<TestEditorPresenter>().AsSingle();
        Container.Bind<QuestionTablePresenter>().AsSingle();
    }
}
