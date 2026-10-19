using System;
using System.Collections.Generic;
using System.Linq;

public static class TestSessionTestDataFactory
{
    public static TestSessionDTO Create(
        TestDTO test,
        int unansweredQuestionIndex = -1)
    {
        var answers = new List<UserBaseAnswerDTO>();

        for (int i = 0; i < test.Questions.Count; i++)
        {
            // Этот вопрос специально оставляем без ответа.
            if (i == unansweredQuestionIndex)
                continue;

            var question = test.Questions[i];

            var answer = CreateAnswer(question);

            if (answer != null)
                answers.Add(answer);
        }

        return new TestSessionDTO
        {
            Answers = answers
        };
    }

    private static UserBaseAnswerDTO CreateAnswer(
        BaseQuestionDTO question)
    {
        switch (question)
        {
            case SingleChoiceQuestionDTO singleChoice:
                return CreateSingleChoice(singleChoice);

            case MultipleChoiceQuestionDTO multipleChoice:
                return CreateMultipleChoice(multipleChoice);

            case TrueFalseQuestionDTO trueFalse:
                return CreateTrueFalse(trueFalse);

            case OpenAnswerQuestionDTO openAnswer:
                return CreateOpenAnswer(openAnswer);

            case OrderingQuestionDTO ordering:
                return CreateOrdering(ordering);

            case MatchingQuestionDTO matching:
                return CreateMatching(matching);

            default:
                return null;
        }
    }

    private static UserSingleChoiceAnswerDTO CreateSingleChoice(
        SingleChoiceQuestionDTO question)
    {
        // Специально правильный ответ.
        return new UserSingleChoiceAnswerDTO
        {
            QuestionId = question.Id,
            SelectedOptionId = question.CorrectOptionId
        };
    }

    private static UserMultipleChoiceAnswerDTO CreateMultipleChoice(
        MultipleChoiceQuestionDTO question)
    {
        // Специально неправильный ответ.
        var wrongOption = question.Options
            .FirstOrDefault(option =>
                !question.CorrectOptionIds.Contains(option.Id));

        if (wrongOption == null)
        {
            return new UserMultipleChoiceAnswerDTO
            {
                QuestionId = question.Id,
                SelectedOptionIds = new List<Guid>()
            };
        }

        return new UserMultipleChoiceAnswerDTO
        {
            QuestionId = question.Id,

            SelectedOptionIds = new List<Guid>
            {
                wrongOption.Id
            }
        };
    }

    private static UserTrueFalseAnswerDTO CreateTrueFalse(
        TrueFalseQuestionDTO question)
    {
        // Специально правильный ответ.
        return new UserTrueFalseAnswerDTO
        {
            QuestionId = question.Id,
            SelectedOptionId = question.CorrectOptionId
        };
    }

    private static UserOpenAnswerDTO CreateOpenAnswer(
        OpenAnswerQuestionDTO question)
    {
        // Специально неправильный ответ.
        return new UserOpenAnswerDTO
        {
            QuestionId = question.Id,
            Text = "Неправильный тестовый ответ"
        };
    }

    private static UserOrderingAnswerDTO CreateOrdering(
        OrderingQuestionDTO question)
    {
        // Специально правильный порядок.
        return new UserOrderingAnswerDTO
        {
            QuestionId = question.Id,

            Items = question.Options
                .OrderBy(option => option.Order)
                .Select((option, index) =>
                    new UserOrderingItemDTO
                    {
                        OptionId = option.Id,
                        Order = index + 1
                    })
                .ToList()
        };
    }

    private static UserMatchingAnswerDTO CreateMatching(
        MatchingQuestionDTO question)
    {
        // Специально неправильное сопоставление.
        if (question.Pairs.Count < 2)
        {
            return new UserMatchingAnswerDTO
            {
                QuestionId = question.Id,
                Pairs = new List<UserMatchingPairAnswerDTO>()
            };
        }

        var pairs = new List<UserMatchingPairAnswerDTO>();

        for (int i = 0; i < question.Pairs.Count; i++)
        {
            int wrongIndex =
                (i + 1) % question.Pairs.Count;

            pairs.Add(
                new UserMatchingPairAnswerDTO
                {
                    LeftPairId = question.Pairs[i].Id,
                    RightPairId =
                        question.Pairs[wrongIndex].Id
                });
        }

        return new UserMatchingAnswerDTO
        {
            QuestionId = question.Id,
            Pairs = pairs
        };
    }
}