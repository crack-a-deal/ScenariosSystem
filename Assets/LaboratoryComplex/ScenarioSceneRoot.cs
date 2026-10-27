using System;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSceneRoot : MonoBehaviour
{
    [SerializeField] private Camera _playerCamera;

    public Camera PlayerCamera => _playerCamera;

    public event Action ExitRequested;

    private void Awake()
    {
        if (_playerCamera != null)
        {
            _playerCamera.enabled = false;

            AudioListener listener = _playerCamera.GetComponent<AudioListener>();

            if (listener != null)
                listener.enabled = false;
        }
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
}
