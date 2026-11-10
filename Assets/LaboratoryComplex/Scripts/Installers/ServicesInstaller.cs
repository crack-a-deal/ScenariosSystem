using PasswordHasher;
using Zenject;

public class ServicesInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IPasswordHasher>()
            .To<PasswordHasherService>()
            .AsSingle();

        Container.Bind<IUserAccountRepository>()
            .To<UserAccountRepository>()
            .AsSingle();

        Container.Bind<IUserProfileRepository>()
            .To<UserProfileRepository>()
            .AsSingle();

        Container.Bind<IUserGroupRepository>()
            .To<UserGroupRepository>()
            .AsSingle();



        Container.Bind<IUserAccountService>()
            .To<UserAccountService>()
            .AsSingle();

        Container.Bind<IUserProfileService>()
            .To<UserProfileService>()
            .AsSingle();

        Container.Bind<IUserGroupService>()
            .To<UserGroupService>()
            .AsSingle();

        Container.Bind<IValidateUsersService>()
            .To<ValidateUserService>()
            .AsSingle();




        Container.Bind<IThreeDModelRepository>()
            .To<ThreeDModelRepository>()
            .AsSingle();

        Container.Bind<IThreeDModelService>()
            .To<ThreeDModelService>()
            .AsSingle();

        Container.Bind<IScenarioRepository>()
            .To<ScenarioRepository>()
            .AsSingle();

        Container.Bind<IScenarioService>()
            .To<ScenarioService>()
            .AsSingle();



        // Tests
        Container.Bind<TestRepository>()
            .AsSingle();

        Container.Bind<TestResultService>()
            .AsSingle();

        Container.Bind<ITestService>()
            .To<TestService>()
            .AsSingle();

        Container.Bind<TestSessionRepository>().AsSingle();

        Container.Bind<TestSessionService>().AsSingle();

        Container.Bind<JsonTestParser>().AsSingle();
        Container.Bind<JsonTestMapper>().AsSingle();

        Container.Bind<JsonTestImportService>().AsSingle();
        Container.Bind<RandomTestGenerator>().AsSingle();


        Container.Bind<UserAnswerSaveService>()
            .FromMethod(ctx => UserAnswerSaveServiceFactory.Create(
                ctx.Container.Resolve<AppDbContext>()))
            .AsSingle();


        Container.Bind<IExcelExportService>()
            .To<ExcelExportService>()
            .AsSingle();

        Container.Bind<IExportPathProvider>()
            .To<LinuxExportPathProvider>()
            .AsSingle();

        Container.Bind<ExcelExportRegistry>()
            .FromMethod(CreateExcelExportRegistry)
            .AsSingle();

        Container.Bind<MultiSheetExcelExportRegistry>()
            .FromMethod(CreateMultiSheetExcelExportRegistry)
            .AsSingle();
    }


    private ExcelExportRegistry CreateExcelExportRegistry()
    {
        var registry = new ExcelExportRegistry();

        registry.Register(new UserAccountExcelDefinition(new UserAccountExcelMapper()));
        registry.Register(new UserGroupExcelDefinition(new UserGroupExcelMapper()));
        registry.Register(new UserProfileExcelDefinition(new UserProfileExcelMapper()));
        
        return registry;
    }

    private MultiSheetExcelExportRegistry CreateMultiSheetExcelExportRegistry()
    {
        var registry = new MultiSheetExcelExportRegistry();

        registry.Register(
            new TestExcelDefinition(
                new TestExcelMapper(),
                new QuestionExcelMapper(),
                new AnswerOptionExcelMapper(),
                new MatchingPairExcelMapper()));

        registry.Register(
            new TestSessionExcelDefinition(
                new TestSessionResultExcelMapper()));

        registry.Register(new ViewedModelsExcelDefinition(new ViewedModelExcelMapper()));
        registry.Register(new ScenarioCompletionsExcelDefinition(new ScenarioCompletionExcelMapper()));

        return registry;
    }
}
