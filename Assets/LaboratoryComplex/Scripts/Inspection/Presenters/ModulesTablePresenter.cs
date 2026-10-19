using System;
using System.Collections.Generic;
using Zenject;

public class ModulesTablePresenter : IDisposable
{
    public event Action BackClicked;
    public event Action<string> ModuleSelected;
    private readonly ModulesTableView _view;
    private readonly ModulesList _modulesList;

    private readonly Dictionary<ModuleItemView, string> _rows = new();

    public ModulesTablePresenter([Inject(Id = "ModulesModule")] ModulesTableView view, ModulesList modulesList)
    {
        _view = view;
        _modulesList = modulesList;

        _view.BackClicked += OnBackClicked;
    }

    public void Dispose()
    {
        _view.BackClicked -= OnBackClicked;
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
        var modules = _modulesList.Modules;
        _view.Clear();
        _rows.Clear();

        for (int i = 0; i < modules.Length; i++)
        {
            string module = modules[i];
            AddRow(module, i);
        }
    }
    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void AddRow(string module, int index)
    {
        var row = _view.CreateRow();

        _rows[row] = module;

        row.SetTitle(module);
        row.SetIndex((index + 1).ToString("D2"));

        row.ItemClicked += button => HandleButton(row);
    }

    private void HandleButton(ModuleItemView row)
    {
        if (!_rows.TryGetValue(row, out var module))
            return;
        ModuleSelected?.Invoke(module);
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}