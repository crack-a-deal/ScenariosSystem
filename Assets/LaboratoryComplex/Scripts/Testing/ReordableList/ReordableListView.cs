using System;
using System.Collections.Generic;
using UnityEngine;

public class ReordableListView : MonoBehaviour
{
    public event Action<int> UpClicked;
    public event Action<int> DownClicked;
    public event Action<int, string> ItemTextChanged;

    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ReorableItemView itemPrefab;

    private readonly List<ReorableItemView> _items = new();

    private void OnEnable()
    {
        foreach (ReorableItemView item in _items)
            Subscribe(item);
    }

    private void OnDisable()
    {
        foreach (ReorableItemView item in _items)
            Unsubscribe(item);
    }

    public void SetItems(IReadOnlyList<ReordableItemModel> items)
    {
        Clear();

        for (int i = 0; i < items.Count; i++)
        {
            ReorableItemView item = Instantiate(itemPrefab, itemsContainer);
            item.SetText(items[i].Text);
            _items.Add(item);
            Subscribe(item);
        }

        RefreshIndices();
    }

    public void Clear()
    {
        foreach (ReorableItemView item in _items)
        {
            Unsubscribe(item);
            Destroy(item.gameObject);
        }

        _items.Clear();
    }

    public void RefreshIndices()
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i].SetIndexText((i + 1).ToString());
    }

    private void Subscribe(ReorableItemView item)
    {
        item.UpClicked -= OnUpClicked;
        item.DownClicked -= OnDownClicked;
        item.TextChanged -= OnTextChanged;

        item.UpClicked += OnUpClicked;
        item.DownClicked += OnDownClicked;
        item.TextChanged += OnTextChanged;
    }

    private void Unsubscribe(ReorableItemView item)
    {
        item.UpClicked -= OnUpClicked;
        item.DownClicked -= OnDownClicked;
        item.TextChanged -= OnTextChanged;
    }

    private void OnUpClicked(ReorableItemView item)
    {
        int index = _items.IndexOf(item);

        if (index >= 0)
            UpClicked?.Invoke(index);
    }

    private void OnDownClicked(ReorableItemView item)
    {
        int index = _items.IndexOf(item);

        if (index >= 0)
            DownClicked?.Invoke(index);
    }

    private void OnTextChanged(ReorableItemView item, string text)
    {
        int index = _items.IndexOf(item);

        if (index >= 0)
            ItemTextChanged?.Invoke(index, text);
    }
}