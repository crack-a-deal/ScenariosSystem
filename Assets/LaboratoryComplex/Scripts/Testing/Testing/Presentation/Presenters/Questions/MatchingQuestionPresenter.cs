
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchingQuestionPresenter : IQuestionPresenter
{
    private readonly MatchingQuestionView _view;
    private readonly MatchingQuestionDTO _question;

    private ReordableListPresenter _leftPresenter;
    private ReordableListPresenter _rightPresenter;

    private List<Guid> _leftOrder;
    private List<Guid> _rightOrder;

    public BaseQuestionDTO Question => _question;

    public MatchingQuestionPresenter(MatchingQuestionView view, MatchingQuestionDTO question)
    {
        _view = view;
        _question = question;
    }

    public void Show()
    {
        _view.SetQuestionText(_question.Text);

        var leftItems = _question.Pairs
            .Select(x => new ReordableItemModel(x.Id, x.LeftText))
            .ToList();

        var rightItems = _question.Pairs
            .Select(x => new ReordableItemModel(x.Id, x.RightText))
            .ToList();

        ShuffleOptions(leftItems);
        ShuffleOptions(rightItems);

        Debug.Log($"Left: {string.Join(", ", leftItems.Select(x => x.Text))}");
        Debug.Log($"Right: {string.Join(", ", rightItems.Select(x => x.Text))}");

        _leftOrder = leftItems.Select(x => x.Id).ToList();
        _rightOrder = rightItems.Select(x => x.Id).ToList();

        _leftPresenter = new ReordableListPresenter(
            _view.LeftReordableList,
            leftItems);

        _rightPresenter = new ReordableListPresenter(
            _view.RightReordableList,
            rightItems);

        _leftPresenter.OrderChanged += OnLeftOrderChanged;
        _rightPresenter.OrderChanged += OnRightOrderChanged;
    }

    private void OnLeftOrderChanged(IReadOnlyList<Guid> order)
    {
        _leftOrder = order.ToList();
    }

    private void OnRightOrderChanged(IReadOnlyList<Guid> order)
    {
        _rightOrder = order.ToList();
    }

    public UserBaseAnswerDTO GetAnswer()
    {
        var pairs = new List<UserMatchingPairAnswerDTO>();

        for (int i = 0; i < _leftOrder.Count; i++)
        {
            pairs.Add(new UserMatchingPairAnswerDTO
            {
                LeftPairId = _leftOrder[i],
                RightPairId = _rightOrder[i]
            });
        }

        return new UserMatchingAnswerDTO
        {
            QuestionId = _question.Id,
            Pairs = pairs
        };
    }

    private static void ShuffleOptions(List<ReordableItemModel> items)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
