
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class BootstrapState : BaseApplicationState
{
    private readonly JsonTestImportService _testImportService;
    private readonly IThreeDModelService _modelService;
    private readonly ModulesList _modulesList;

    public BootstrapState(
        IApplication application,
        JsonTestImportService testImportService, IThreeDModelService modelService, ModulesList modulesList)
        : base(application)
    {
        _testImportService = testImportService;
        _modelService = modelService;
        _modulesList = modulesList;
    }

    public override async Task EnterAsync()
    {
        await LoadTestFromConfig();
        await InitModels();

        await _application.ChangeStateAsync<RoleSelectionState>();
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
}
