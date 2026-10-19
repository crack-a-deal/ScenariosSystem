using UnityEngine;

public class QuestionEditViewContainer : MonoBehaviour
{
    [SerializeField] private ChoiceQuestionEditorView singleChoice;
    [SerializeField] private ChoiceQuestionEditorView multipleChoice;
    [SerializeField] private ChoiceQuestionEditorView trueFalse;
    [SerializeField] private OpenQuestionEditorView open;
    [SerializeField] private OrderingQuestionEditorView ordering;
    [SerializeField] private MatchingQuestionEditorView matching;

    private GameObject _currentView;

    public ChoiceQuestionEditorView SingleChoice => singleChoice;
    public ChoiceQuestionEditorView MultipleChoice => multipleChoice;
    public ChoiceQuestionEditorView TrueFalse => trueFalse;
    public OpenQuestionEditorView Open => open;
    public OrderingQuestionEditorView Ordering => ordering;
    public MatchingQuestionEditorView Matching => matching;

    public void Show(QuestionType type)
    {
        if (_currentView != null)
        {
            _currentView.SetActive(false);
        }

        switch (type)
        {
            case QuestionType.SingleChoice:
                _currentView = singleChoice.gameObject;
                break;
            case QuestionType.MultipleChoice:
                _currentView = multipleChoice.gameObject;
                break;
            case QuestionType.TrueFalse:
                _currentView = trueFalse.gameObject;
                break;
            case QuestionType.OpenAnswer:
                _currentView = open.gameObject;
                break;
            case QuestionType.Matching:
                _currentView = matching.gameObject;
                break;
            case QuestionType.Ordering:
                _currentView = ordering.gameObject;
                break;
            default:
                break;
        }

        _currentView.SetActive(true);
    }
}
