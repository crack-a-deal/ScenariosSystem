using System;
using UnityEngine;

public class ExitPresenter : IDisposable
{
    private readonly ExitView _view;

    public ExitPresenter(ExitView view)
    {
        _view = view;
        _view.ExitClicked += OnExitClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    public void Dispose()
    {
        _view.ExitClicked -= OnExitClicked;
        _view.CancelClicked -= OnCancelClicked;
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnExitClicked()
    {
        ExitApplication();
    }

    private void OnCancelClicked()
    {
        Hide();
    }
}