using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class ViewsInstaller : MonoInstaller
{
    [Header("Application")]
    [SerializeField] private MainView mainView;
    [SerializeField] private ExitView exitView;


    [Header("Authentication")]
    [SerializeField] private RoleSelectionView roleSelectionView;
    [SerializeField] private LoginView adminLoginView;
    [SerializeField] private LoginView studentLoginView;
    [SerializeField] private StudentWaitingView studentWaitingView;
    [SerializeField] private LearningSectionsView studentLearningSectionsView;
    [SerializeField] private LearningSectionsView adminLearningSectionsView;


    [Header("Administration")]
    [SerializeField] private AdministrationPanelView administrationPanel;
    [SerializeField] private AdminTablesView adminTables;
    [SerializeField] private ComputersTableView computersTable;
    [SerializeField] private StudentsTableView studentsTable;
    [SerializeField] private GroupsTableView groupsTable;
    [SerializeField] private TestsTableView testsTable;

    [SerializeField] private StudentFormView addStudent;
    [SerializeField] private GroupFormView addGroup;


    [Header("Admin Screen")]
    [SerializeField] private ModulesTableView adminTestTable;


    [Header("Student Screen")]
    [SerializeField] private ModulesTableView modulesTable;
    [SerializeField] private ModulesTableView studentTestTable;
    [SerializeField] private ModulesList modulesList;
    [SerializeField] private TestSessionView testSessionView;
    [SerializeField] private TestResultsView testResults;

    [SerializeField] private QuestionViewContainer viewContainer;
    

    [Header("Test Edit")]
    [SerializeField] private TestEditorPanelView testEditorPanel;
    [SerializeField] private QuestionTableView questionEditorTable;
    [SerializeField] private TestEditorView testEditorView;
    [SerializeField] private QuestionEditViewContainer questionEditViewContainer;




    public override void InstallBindings()
    {
        // Application

        Container.Bind<MainView>()
            .FromInstance(mainView)
            .AsSingle();

        Container.Bind<ExitView>()
            .FromInstance(exitView)
            .AsSingle();

        // Authentication

        Container.Bind<RoleSelectionView>()
           .FromInstance(roleSelectionView)
           .AsSingle();

        Container.Bind<LoginView>()
            .WithId("Admin")
            .FromInstance(adminLoginView)
            .AsCached();

        Container.Bind<LoginView>()
            .WithId("Student")
            .FromInstance(studentLoginView)
            .AsCached();

        Container.Bind<StudentWaitingView>()
           .FromInstance(studentWaitingView)
           .AsSingle();

        Container.Bind<LearningSectionsView>()
           .WithId("Admin")
           .FromInstance(adminLearningSectionsView)
           .AsCached();

        Container.Bind<LearningSectionsView>()
            .WithId("Student")
            .FromInstance(studentLearningSectionsView)
            .AsCached();

        // Administration

        Container.Bind<AdministrationPanelView>()
            .FromInstance(administrationPanel)
            .AsSingle();

        Container.Bind<AdminTablesView>()
            .FromInstance(adminTables)
            .AsSingle();

        Container.Bind<ComputersTableView>()
            .FromInstance(computersTable)
            .AsSingle();

        Container.Bind<StudentsTableView>()
            .FromInstance(studentsTable)
            .AsSingle();

        Container.Bind<GroupsTableView>()
            .FromInstance(groupsTable)
            .AsSingle();

        Container.Bind<TestsTableView>()
            .FromInstance(testsTable)
            .AsSingle();


        Container.Bind<StudentFormView>()
            .FromInstance(addStudent)
            .AsSingle();

        Container.Bind<GroupFormView>()
            .FromInstance(addGroup)
            .AsSingle();

        // Student

        Container.Bind<ModulesTableView>()
            .WithId("ModulesModule")
            .FromInstance(modulesTable)
            .AsCached();

        Container.Bind<ModulesTableView>()
            .WithId("StudentTestModule")
            .FromInstance(studentTestTable)
            .AsCached();

        Container.Bind<ModulesTableView>()
            .WithId("AdminTestModule")
            .FromInstance(adminTestTable)
            .AsCached();

        Container.Bind<ModulesList>()
            .FromInstance(modulesList)
            .AsSingle();

        Container.Bind<TestSessionView>()
            .FromInstance(testSessionView)
            .AsSingle();

        Container.Bind<TestResultsView>()
            .FromInstance(testResults)
            .AsSingle();

        Container.Bind<QuestionViewContainer>()
            .FromInstance(viewContainer)
            .AsSingle();

        Container.Bind<TestEditorPanelView>()
            .FromInstance(testEditorPanel)
            .AsSingle();

        Container.Bind<QuestionTableView>()
            .FromInstance(questionEditorTable)
            .AsSingle();

        Container.Bind<TestEditorView>()
            .FromInstance(testEditorView)
            .AsSingle();

        Container.Bind<QuestionEditViewContainer>()
            .FromInstance(questionEditViewContainer)
            .AsSingle();
    }
}
