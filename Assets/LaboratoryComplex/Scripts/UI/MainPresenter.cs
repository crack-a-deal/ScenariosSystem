using System;
using UnityEngine;

public class MainPresenter : IDisposable
{
    private readonly MainView _view;
    private readonly ExitPresenter _exitPresenter;

    public MainPresenter(MainView view, ExitPresenter exitPresenter)
    {
        Debug.Log("MainPresenter");
        _view = view;
        _exitPresenter = exitPresenter;
        _view.CloseClicked += OnCloseClicked;
    }

    public void Dispose()
    {
        _view.CloseClicked -= OnCloseClicked;
    }

    private void OnCloseClicked()
    {
        _exitPresenter.Show();
    }
}