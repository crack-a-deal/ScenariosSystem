using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class BootstrapState : BaseApplicationState
{
    private readonly JsonTestImportService _testImportService;
    private readonly IThreeDModelService _modelService;
    private readonly ModulesList _modulesList;
    private readonly GuardantLicenseService _licenseService;
    private readonly IScenarioService _scenarioService;
    private readonly ScenarioList _scenarioList;

    public BootstrapState(
        IApplication application,
        JsonTestImportService testImportService, IThreeDModelService modelService, ModulesList modulesList, GuardantLicenseService licenseService, IScenarioService scenarioService,
        ScenarioList scenarioList)
        : base(application)
    {
        _testImportService = testImportService;
        _modelService = modelService;
        _modulesList = modulesList;
        _licenseService = licenseService;
        _scenarioService = scenarioService;
        _scenarioList = scenarioList;
    }

    public override async Task EnterAsync()
    {
        _licenseService.CheckNow();

        if (_licenseService.State != GuardantLicenseState.Licensed)
        {
            await _application.ChangeStateAsync<StudentWaitingState>();
            return;
        }


        await LoadTestFromConfig();
        await InitModels();
        await InitScenarios();

        await _application.ChangeStateAsync<TeacherAuthorizationState>();
    }

    public override Task ExitAsync()
    {
        return Task.CompletedTask;
    }

    private async Task LoadTestFromConfig()
    {
        try
        {
            TestsConfig config = TestsConfigLoader.Load();

            string path = Path.Combine(
                Application.streamingAssetsPath,
                config.testBankFile);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Test bank not found: {path}");
            }

            bool imported = await _testImportService.ImportAsync(path);

            if (imported)
            {
                Debug.Log("Test bank imported successfully.");
            }
            else
            {
                Debug.LogWarning("Test bank import returned false.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    private async Task InitModels()
    {
        await _modelService.InitializeCatalogAsync(_modulesList.Modules);
        Debug.Log("База данных и каталог 3D-моделей готовы.");
    }

    private async Task InitScenarios()
    {
        await _scenarioService.InitializeCatalogAsync(_scenarioList.Scenarios);
        Debug.Log("Каталог сценариев готов.");
    }
}
