using System;
using System.Collections.Generic;
using System.Linq;

public class OrderingQuestionEditorPresenter : IQuestionEditorPresenter
{
    public event Action CancelClicked;
    public event Action<BaseQuestionDTO> Saved;

    private readonly OrderingQuestionEditorView _view;
    private readonly List<QuestionOptionModel> _options = new();

    private ReordableListPresenter _listPresenter;

    private Guid _questionId;
    private int _order;
    private string _questionText = string.Empty;

    public BaseQuestionDTO Question => GetQuestion();

    public OrderingQuestionEditorPresenter(OrderingQuestionEditorView view, OrderingQuestionDTO question)
    {
        _view = view;

        Subscribe();
        Load(question);
    }

    private void Subscribe()
    {
        _view.QuestionTextChanged += OnQuestionTextChanged;
        _view.AddOptionClicked += OnAddOptionClicked;
        _view.SaveClicked += OnSaveClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    private void Load(OrderingQuestionDTO question)
    {
        _questionId = question.Id;
        _order = question.Order;
        _questionText = question.Text ?? string.Empty;

        _options.Clear();

        foreach (AnswerOptionDTO option in question.Options ?? new List<AnswerOptionDTO>())
            _options.Add(new QuestionOptionModel(option.Id, option.Text ?? string.Empty));
    }

    public void Show()
    {
        _view.SetQuestionText(_questionText);
        _view.ClearError();

        if (_listPresenter == null)
        {
            _listPresenter = new ReordableListPresenter(_view.OptionList, GetItems());
            _listPresenter.OrderChanged += OnOrderChanged;
            _listPresenter.ItemTextChanged += OnOptionTextChanged;
            _listPresenter.ItemRemoved += OnOptionRemoved;
        }
        else
        {
            _listPresenter.SetItems(GetItems());
        }
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void OnOptionRemoved(Guid optionId)
    {
        QuestionOptionModel option = _options.FirstOrDefault(x => x.Id == optionId);

        if (option != null)
            _options.Remove(option);
    }

    private List<ReordableItemModel> GetItems()
    {
        return _options
            .Select(x => new ReordableItemModel(x.Id, x.Text))
            .ToList();
    }

    private void OnOrderChanged(IReadOnlyList<Guid> order)
    {
        var optionById = _options.ToDictionary(x => x.Id);

        List<QuestionOptionModel> orderedOptions = new();

        foreach (Guid id in order)
        {
            if (optionById.TryGetValue(id, out QuestionOptionModel option))
                orderedOptions.Add(option);
        }

        foreach (QuestionOptionModel option in _options)
        {
            if (!orderedOptions.Any(x => x.Id == option.Id))
                orderedOptions.Add(option);
        }

        _options.Clear();
        _options.AddRange(orderedOptions);
    }

    private void OnOptionTextChanged(Guid optionId, string text)
    {
        QuestionOptionModel option = _options.FirstOrDefault(x => x.Id == optionId);

        if (option != null)
            option.Text = text;
    }

    private void OnQuestionTextChanged(string text)
    {
        _questionText = text;
    }

    private void OnAddOptionClicked()
    {
        _options.Add(new QuestionOptionModel(Guid.NewGuid(), string.Empty));
        Show();
    }

    public bool Validate()
    {
        return string.IsNullOrWhiteSpace(GetValidationError());
    }

    public BaseQuestionDTO GetQuestion()
    {
        return new OrderingQuestionDTO
        {
            Id = _questionId,
            Text = _questionText,
            Type = QuestionType.Ordering,
            Order = _order,
            Options = _options.Select((x, index) => new AnswerOptionDTO
            {
                Id = x.Id,
                Text = x.Text,
                Order = index + 1
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

        if (_options.Count < 2)
            return "Добавьте минимум два варианта.";

        if (_options.Any(x => string.IsNullOrWhiteSpace(x.Text)))
            return "Заполните текст всех вариантов.";

        return string.Empty;
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public void Dispose()
    {
        _view.QuestionTextChanged -= OnQuestionTextChanged;
        _view.AddOptionClicked -= OnAddOptionClicked;
        _view.SaveClicked -= OnSaveClicked;
        _view.CancelClicked -= OnCancelClicked;

        if (_listPresenter == null)
            return;

        _listPresenter.OrderChanged -= OnOrderChanged;
        _listPresenter.ItemTextChanged -= OnOptionTextChanged;
        _listPresenter.ItemRemoved -= OnOptionRemoved;

        _listPresenter.Dispose();
        _listPresenter = null;
    }
}