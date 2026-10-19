using System;

public class QuestionPresenterFactory
{
    private readonly QuestionViewContainer _viewContainer;

    public QuestionPresenterFactory(QuestionViewContainer viewContainer)
    {
        _viewContainer = viewContainer;
    }

    public IQuestionPresenter Create(BaseQuestionDTO question)
    {
        return question switch
        {
            SingleChoiceQuestionDTO => new SingleChoiceQuestionPresenter(_viewContainer.SingleChoice, (SingleChoiceQuestionDTO)question),
            MultipleChoiceQuestionDTO => new MultipleChoiceQuestionPresenter(_viewContainer.MultipleChoice, (MultipleChoiceQuestionDTO)question),
            TrueFalseQuestionDTO => new TrueFalseQuestionPresenter(_viewContainer.TrueFalse, (TrueFalseQuestionDTO)question),
            OpenAnswerQuestionDTO => new OpenQuestionPresenter(_viewContainer.Open, (OpenAnswerQuestionDTO)question),
            OrderingQuestionDTO => new OrderingQuestionPresenter(_viewContainer.Ordering, (OrderingQuestionDTO)question),
            MatchingQuestionDTO => new MatchingQuestionPresenter(_viewContainer.Matching, (MatchingQuestionDTO)question),
            _ => throw new NotImplementedException()
        };
    }
}

public class QuestionEditPresenterFactory
{
    private readonly QuestionEditViewContainer _viewContainer;

    public QuestionEditPresenterFactory(QuestionEditViewContainer viewContainer)
    {
        _viewContainer = viewContainer;
    }

    public IQuestionEditorPresenter Create(BaseQuestionDTO question)
    {
        return question switch
        {
            SingleChoiceQuestionDTO => new SingleChoiceQuestionEditorPresenter(_viewContainer.SingleChoice, (SingleChoiceQuestionDTO)question),
            MultipleChoiceQuestionDTO => new MultipleChoiceQuestionEditorPresenter(_viewContainer.MultipleChoice, (MultipleChoiceQuestionDTO)question),
            TrueFalseQuestionDTO => new TrueFalseQuestionEditorPresenter(_viewContainer.TrueFalse, (TrueFalseQuestionDTO)question),
            OpenAnswerQuestionDTO => new OpenQuestionEditorPresenter(_viewContainer.Open, (OpenAnswerQuestionDTO)question),
            OrderingQuestionDTO => new OrderingQuestionEditorPresenter(_viewContainer.Ordering, (OrderingQuestionDTO)question),
            MatchingQuestionDTO => new MatchingQuestionEditorPresenter(_viewContainer.Matching, (MatchingQuestionDTO)question),
            _ => throw new NotImplementedException()
        };
    }
}