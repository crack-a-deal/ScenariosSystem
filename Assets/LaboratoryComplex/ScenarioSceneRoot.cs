using System;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSceneRoot : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private ScenarioRunner _scenarioRunner;

    public Camera PlayerCamera => _playerCamera;

    public event Action ExitRequested;
    public event Action<string> ScenarioCompleted;

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

    private void OnScenarioCompleted()
    {
        ScenarioCompleted?.Invoke(_scenarioRunner.ScenarioTitle);
    }
}
