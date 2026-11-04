using UnityEngine;

public class QuestionViewContainer : MonoBehaviour
{
    [SerializeField] private ChoiceQuestionView singleChoice;
    [SerializeField] private ChoiceQuestionView multipleChoice;
    [SerializeField] private ChoiceQuestionView trueFalse;
    [SerializeField] private OpenQuestionView open;
    [SerializeField] private OrderingQuestionView ordering;
    [SerializeField] private MatchingQuestionView matching;

    private GameObject _currentView;

    public ChoiceQuestionView SingleChoice => singleChoice;
    public ChoiceQuestionView MultipleChoice => multipleChoice;
    public ChoiceQuestionView TrueFalse => trueFalse;
    public OpenQuestionView Open => open;
    public OrderingQuestionView Ordering => ordering;
    public MatchingQuestionView Matching => matching;

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
                Debug.LogError($"QuestionViewContainer: неизвестный тип вопроса {type}.");
                return;
        }

        _currentView.SetActive(true);
    }
}
