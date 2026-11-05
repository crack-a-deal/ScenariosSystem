using System;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioCompletionInfo
{
    public string ScenarioName { get; }
    public ScenarioMode Mode { get; }
    public double DurationSeconds { get; }

    public ScenarioCompletionInfo(
        string scenarioName,
        ScenarioMode mode,
        double durationSeconds)
    {
        ScenarioName = scenarioName;
        Mode = mode;
        DurationSeconds = durationSeconds;
    }
}

public class ScenarioSceneRoot : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private ScenarioRunner _scenarioRunner;

    public Camera PlayerCamera => _playerCamera;

    public event Action ExitRequested;
    public event Action<ScenarioCompletionInfo> ScenarioCompleted;

    private void Awake()
    {
        if (_playerCamera != null)
        {
            _playerCamera.enabled = false;

            AudioListener listener = _playerCamera.GetComponent<AudioListener>();

            if (listener != null)
                listener.enabled = false;
        }
        if (_scenarioRunner != null)
            _scenarioRunner.ScenarioCompleted += OnScenarioCompleted;
    }

    private void OnDestroy()
    {
        if (_scenarioRunner != null)
            _scenarioRunner.ScenarioCompleted -= OnScenarioCompleted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("RequestExit");
            RequestExit();
        }
    }

    public void RequestExit()
    {
        ExitRequested?.Invoke();
    }

    public void SetMode(ScenarioMode mode)
    {
        if (_scenarioRunner != null)
            _scenarioRunner.SetMode(mode);
    }

    private void OnScenarioCompleted()
    {
        var completion = new ScenarioCompletionInfo(
            _scenarioRunner.ScenarioTitle,
            _scenarioRunner.Mode,
            _scenarioRunner.ElapsedSeconds);

        Debug.Log(
            $"OnScenarioCompleted {completion.ScenarioName}, " +
            $"mode: {completion.Mode}, " +
            $"duration: {completion.DurationSeconds:F1}s");

        ScenarioCompleted?.Invoke(completion);
    }
}
