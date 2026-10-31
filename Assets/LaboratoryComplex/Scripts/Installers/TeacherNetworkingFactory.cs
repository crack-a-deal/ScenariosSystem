using Networking;

public class TeacherNetworkingFactory
{
    private readonly NetworkingConfig _config;
    private readonly IMessageSerializer _serializer;
    private readonly IValidateUsersService _validateUsersService;
    private readonly IUserProfileService _profileService;
    private readonly ClientRegistry _clientRegistry;
    private readonly ITestService _testService;
    private readonly TestSessionService _testSession;
    private readonly UserAnswerSaveService _answerSaveService;
    private readonly TestResultService _testResultService;
    private readonly IThreeDModelService _threeDModel;
    private readonly IScenarioService _scenarioService;

    public TeacherNetworkingFactory(
        NetworkingConfig config,
        IMessageSerializer serializer,
        IValidateUsersService validateUsersService,
        IUserProfileService profileService,
        ClientRegistry clientRegistry,
        ITestService testService,
        TestSessionService testSession,
        UserAnswerSaveService answerSaveService,
        TestResultService testResultService,IThreeDModelService threeDModel,IScenarioService scenarioService)
    {
        _config = config;
        _serializer = serializer;
        _validateUsersService = validateUsersService;
        _profileService = profileService;
        _clientRegistry = clientRegistry;
        _testService = testService;
        _testSession = testSession;
        _answerSaveService = answerSaveService;
        _testResultService = testResultService;
        _threeDModel = threeDModel;
        _scenarioService = scenarioService;
    }

    public TcpServer Create()
    {
        RequestDispatcher dispatcher = new RequestDispatcher(_serializer);

        dispatcher.RegisterHandler(
            new AuthenticationRequestHandler(_validateUsersService, _profileService));

        dispatcher.RegisterHandler(new GetAvailableTestsHandler(_testSession));
        dispatcher.RegisterHandler(new GetTestHandler(_testService));
        dispatcher.RegisterHandler(new StartTestRequestHandler(_testSession));
        dispatcher.RegisterHandler(new Register3DModelViewHandler(_threeDModel));
        dispatcher.RegisterHandler(new RegisterScenarioCompletionHandler(_scenarioService));

        dispatcher.RegisterHandler(
            new SubmitTestRequestHandler(_answerSaveService, _testSession, _testResultService));

        TcpServer server = new TcpServer(
            0,
            _serializer,
            dispatcher,
            _clientRegistry);

        server.RegisterDeferredRequest<AccessRequest>();
        //server.ErrorOccurred += Debug.LogException;

        return server;
    }
}