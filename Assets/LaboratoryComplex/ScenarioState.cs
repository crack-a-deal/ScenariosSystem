using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioState : BaseApplicationState
{
    private const string ScenarioSceneName = "ScenarioScene 1";

    private readonly CameraSwitcher _cameraSwitcher;
    private readonly Canvas _mainCanvas;

    private ScenarioSceneRoot _scenarioRoot;
    private Scene _scenarioScene;
    private bool _isExiting;

    public ScenarioState(
        IApplication application,
        CameraSwitcher cameraSwitcher,
        [Zenject.Inject(Id = "MainCanvas")] Canvas mainCanvas)
        : base(application)
    {
        _cameraSwitcher = cameraSwitcher;
        _mainCanvas = mainCanvas;
    }

    public override async Task EnterAsync()
    {
        _isExiting = false;

        _mainCanvas.enabled = false;

        await SceneManager.LoadSceneAsync(
            ScenarioSceneName,
            LoadSceneMode.Additive);

        _scenarioScene = SceneManager.GetSceneByName(ScenarioSceneName);
        SceneManager.SetActiveScene(_scenarioScene);

        _scenarioRoot = FindScenarioRoot(_scenarioScene);

        if (_scenarioRoot == null)
            throw new InvalidOperationException(
                "В ScenarioScene не найден ScenarioSceneRoot.");

        _scenarioRoot.ExitRequested += OnExitRequested;

        _cameraSwitcher.Activate(_scenarioRoot.PlayerCamera);
    }

    private static ScenarioSceneRoot FindScenarioRoot(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            ScenarioSceneRoot root =
                rootObject.GetComponentInChildren<ScenarioSceneRoot>(true);

            if (root != null)
                return root;
        }

        return null;
    }

    private async void OnExitRequested()
    {
        Debug.Log("OnExitRequested");
        if (_isExiting)
            return;

        _isExiting = true;

        try
        {
            await _application.ChangeStateAsync<StudentMainState>();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            _isExiting = false;
        }
    }

    public override async Task ExitAsync()
    {
        if (_scenarioRoot != null)
            _scenarioRoot.ExitRequested -= OnExitRequested;

        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        Cursor.lockState = CursorLockMode.None;

        if (mainScene.IsValid() && mainScene.isLoaded)
            SceneManager.SetActiveScene(mainScene);

        _cameraSwitcher.RestoreMainCamera();

        if (_scenarioScene.IsValid() && _scenarioScene.isLoaded)
            await SceneManager.UnloadSceneAsync(_scenarioScene);

        _mainCanvas.enabled = true;

        _scenarioRoot = null;
        _scenarioScene = default;
    }
}