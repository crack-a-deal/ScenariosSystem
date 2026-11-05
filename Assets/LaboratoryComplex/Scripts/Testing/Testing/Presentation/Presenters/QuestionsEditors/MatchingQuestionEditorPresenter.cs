using System;
using System.Collections.Generic;
using System.Linq;

public class MatchingQuestionEditorPresenter : IQuestionEditorPresenter
{
    public event Action CancelClicked;
    public event Action<BaseQuestionDTO> Saved;

    private readonly MatchingQuestionEditorView _view;
    private readonly List<MatchingOptionModel> _pairs = new();
    private readonly List<Guid> _leftOrder = new();
    private readonly List<Guid> _rightOrder = new();

    private ReordableListPresenter _leftListPresenter;
    private ReordableListPresenter _rightListPresenter;

    private Guid _questionId;
    private int _order;
    private string _questionText = string.Empty;

    public BaseQuestionDTO Question => GetQuestion();

    public MatchingQuestionEditorPresenter(MatchingQuestionEditorView view, MatchingQuestionDTO question)
    {
        _view = view;

        Subscribe();
        Load(question);
    }

    private void Subscribe()
    {
        _view.QuestionTextChanged += OnQuestionTextChanged;
        _view.AddPairClicked += OnAddPairClicked;
        _view.SaveClicked += OnSaveClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    private void Load(MatchingQuestionDTO question)
    {
        _questionId = question.Id;
        _order = question.Order;
        _questionText = question.Text ?? string.Empty;

        _pairs.Clear();
        _leftOrder.Clear();
        _rightOrder.Clear();

        foreach (MatchingPairDTO pair in question.Pairs ?? new List<MatchingPairDTO>())
        {
            _pairs.Add(new MatchingOptionModel(
                pair.Id,
                pair.LeftText ?? string.Empty,
                pair.RightText ?? string.Empty));

            _leftOrder.Add(pair.Id);
            _rightOrder.Add(pair.Id);
        }
    }

    public void Show()
    {
        _view.SetQuestionText(_questionText);
        _view.ClearError();

        if (_leftListPresenter == null || _rightListPresenter == null)
        {
            _leftListPresenter = new ReordableListPresenter(_view.LeftList, GetLeftItems());
            _rightListPresenter = new ReordableListPresenter(_view.RightList, GetRightItems());

            _leftListPresenter.OrderChanged += OnLeftOrderChanged;
            _rightListPresenter.OrderChanged += OnRightOrderChanged;

            _leftListPresenter.ItemTextChanged += OnLeftTextChanged;
            _rightListPresenter.ItemTextChanged += OnRightTextChanged;

            _leftListPresenter.ItemRemoved += OnPairRemoved;
            _rightListPresenter.ItemRemoved += OnPairRemoved;
        }
        else
        {
            RefreshLists();
        }
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void OnPairRemoved(Guid pairId)
    {
        MatchingOptionModel pair = _pairs.FirstOrDefault(x => x.Id == pairId);

        if (pair != null)
            _pairs.Remove(pair);

        _leftOrder.Remove(pairId);
        _rightOrder.Remove(pairId);

        RefreshLists();
    }

    private List<ReordableItemModel> GetLeftItems()
    {
        var pairById = _pairs.ToDictionary(x => x.Id);

        return _leftOrder
            .Where(pairById.ContainsKey)
            .Select(id => new ReordableItemModel(id, pairById[id].LeftText))
            .ToList();
    }

    private List<ReordableItemModel> GetRightItems()
    {
        var pairById = _pairs.ToDictionary(x => x.Id);

        return _rightOrder
            .Where(pairById.ContainsKey)
            .Select(id => new ReordableItemModel(id, pairById[id].RightText))
            .ToList();
    }

    private void RefreshLists()
    {
        _leftListPresenter.SetItems(GetLeftItems());
        _rightListPresenter.SetItems(GetRightItems());
    }

    private void OnLeftOrderChanged(IReadOnlyList<Guid> order)
    {
        _leftOrder.Clear();
        _leftOrder.AddRange(order);
    }

    private void OnRightOrderChanged(IReadOnlyList<Guid> order)
    {
        _rightOrder.Clear();
        _rightOrder.AddRange(order);
    }

    private void OnLeftTextChanged(Guid pairId, string text)
    {
        MatchingOptionModel pair = _pairs.FirstOrDefault(x => x.Id == pairId);

        if (pair != null)
            pair.LeftText = text;
    }

    private void OnRightTextChanged(Guid pairId, string text)
    {
        MatchingOptionModel pair = _pairs.FirstOrDefault(x => x.Id == pairId);

        if (pair != null)
            pair.RightText = text;
    }

    private void OnQuestionTextChanged(string text)
    {
        _questionText = text;
    }

    private void OnAddPairClicked()
    {
        MatchingOptionModel pair = new MatchingOptionModel(
            Guid.NewGuid(),
            string.Empty,
            string.Empty);

        _pairs.Add(pair);
        _leftOrder.Add(pair.Id);
        _rightOrder.Add(pair.Id);

        RefreshLists();
    }

    public bool Validate()
    {
        return string.IsNullOrWhiteSpace(GetValidationError());
    }

    public BaseQuestionDTO GetQuestion()
    {
        return new MatchingQuestionDTO
        {
            Id = _questionId,
            Text = _questionText,
            Type = QuestionType.Matching,
            Order = _order,
            Pairs = _pairs.Select(x => new MatchingPairDTO
            {
                Id = x.Id,
                LeftText = x.LeftText,
                RightText = x.RightText
            }).ToList()
        };
    }

    private void OnSaveClicked()
    {
        if (Validate())
        {
            _view.ClearError();
            Saved?.Invoke(GetQuestion());
        }
        else
        {
            _view.ShowError(GetValidationError());
        }
    }

    private string GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(_questionText))
            return "Введите текст вопроса.";

        if (_pairs.Count < 2)
            return "Добавьте минимум две пары.";

        if (_pairs.Any(x => string.IsNullOrWhiteSpace(x.LeftText) ||
                            string.IsNullOrWhiteSpace(x.RightText)))
            return "Заполните обе части каждой пары.";

        return string.Empty;
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public void Dispose()
    {
        _view.QuestionTextChanged -= OnQuestionTextChanged;
        _view.AddPairClicked -= OnAddPairClicked;
        _view.SaveClicked -= OnSaveClicked;
        _view.CancelClicked -= OnCancelClicked;

        if (_leftListPresenter != null)
        {
            _leftListPresenter.OrderChanged -= OnLeftOrderChanged;
            _leftListPresenter.ItemTextChanged -= OnLeftTextChanged;
            _leftListPresenter.ItemRemoved -= OnPairRemoved;
            _leftListPresenter.Dispose();
            _leftListPresenter = null;
        }

        if (_rightListPresenter != null)
        {
            _rightListPresenter.OrderChanged -= OnRightOrderChanged;
            _rightListPresenter.ItemTextChanged -= OnRightTextChanged;
            _rightListPresenter.ItemRemoved -= OnPairRemoved;
            _rightListPresenter.Dispose();
            _rightListPresenter = null;
        }
    }
}