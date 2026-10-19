using System;
using System.Collections.Generic;
using System.Linq;

public class ReordableListPresenter : IDisposable
{
    public event Action<IReadOnlyList<Guid>> OrderChanged;
    public event Action<Guid, string> ItemTextChanged;

    private readonly ReordableListView _view;
    private readonly List<ReordableItemModel> _items = new();

    public ReordableListPresenter(ReordableListView view, List<ReordableItemModel> items)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));

        _view.UpClicked += OnUpClicked;
        _view.DownClicked += OnDownClicked;
        _view.ItemTextChanged += OnItemTextChanged;

        SetItems(items);
    }

    public void SetItems(IReadOnlyList<ReordableItemModel> items)
    {
        _items.Clear();

        if (items != null)
            _items.AddRange(items);

        _view.SetItems(_items);
    }

    private void OnUpClicked(int index)
    {
        Swap(index, index - 1);
    }

    private void OnDownClicked(int index)
    {
        Swap(index, index + 1);
    }

    private void Swap(int firstIndex, int secondIndex)
    {
        if (firstIndex < 0 || firstIndex >= _items.Count)
            return;

        if (secondIndex < 0 || secondIndex >= _items.Count)
            return;

        (_items[firstIndex], _items[secondIndex]) =
            (_items[secondIndex], _items[firstIndex]);

        _view.SetItems(_items);
        OrderChanged?.Invoke(_items.Select(x => x.Id).ToList());
    }

    private void OnItemTextChanged(int index, string text)
    {
        if (index < 0 || index >= _items.Count)
            return;

        ReordableItemModel item = _items[index];
        item.Text = text;

        ItemTextChanged?.Invoke(item.Id, text);
    }

    public void Dispose()
    {
        _view.UpClicked -= OnUpClicked;
        _view.DownClicked -= OnDownClicked;
        _view.ItemTextChanged -= OnItemTextChanged;

        _view.Clear();

        OrderChanged = null;
        ItemTextChanged = null;
    }
}