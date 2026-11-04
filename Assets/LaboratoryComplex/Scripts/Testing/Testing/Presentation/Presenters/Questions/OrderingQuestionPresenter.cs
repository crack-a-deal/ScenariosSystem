using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderingQuestionPresenter : IQuestionPresenter
{
    private readonly OrderingQuestionView _view;
    private readonly OrderingQuestionDTO _question;
    private ReordableListPresenter _listPresenter;
    private List<Guid> _selectedOrder;
    public BaseQuestionDTO Question => _question;

    public OrderingQuestionPresenter(OrderingQuestionView view, OrderingQuestionDTO question)
    {
        _view = view;
        _question = question;

        _selectedOrder = ShuffleOptions(_question.Options.Select(x => x.Id).ToList());
    }

    public void Show()
    {
        _view.SetQuestionText(_question.Text);

        var optionsById = _question.Options.ToDictionary(x => x.Id);

        var items = _selectedOrder
            .Select(id => new ReordableItemModel(id, optionsById[id].Text))
            .ToList();
        Debug.Log($"Порядок вариантов: {string.Join(", ", items.Select(x => x.Text))}");
        _listPresenter = new ReordableListPresenter(_view.ReordableList, items);
        _listPresenter.OrderChanged += OnOrderChanged;
    }

    public void Hide()
    {
        _listPresenter?.Dispose();
        _listPresenter = null;
    }


    private void OnOrderChanged(IReadOnlyList<Guid> order)
    {
        _selectedOrder = order.ToList();
    }

    public UserBaseAnswerDTO GetAnswer()
    {
        return new UserOrderingAnswerDTO
        {
            QuestionId = _question.Id,
            Items = _selectedOrder
            .Select((optionId, index) => new UserOrderingItemDTO
            {
                OptionId = optionId,
                Order = index
            })
            .ToList()
        };
    }

    private  List<Guid> ShuffleOptions(List<Guid> optionIds)
    {
        Debug.Log("SHUFLE");
        for (int i = optionIds.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (optionIds[i], optionIds[j]) =
                (optionIds[j], optionIds[i]);
        }

        return optionIds;
    }
}