using System;
using System.Collections.Generic;

public static class QuestionDtoFactory
{
    public static BaseQuestionDTO Create(QuestionType type, int order)
    {
        Guid id = Guid.NewGuid();

        return type switch
        {
            QuestionType.SingleChoice => new SingleChoiceQuestionDTO
            {
                Id = id,
                Text = string.Empty,
                Type = type,
                Order = order,
                Options = new List<AnswerOptionDTO>(),
                CorrectOptionId = null
            },

            QuestionType.MultipleChoice => new MultipleChoiceQuestionDTO
            {
                Id = id,
                Text = string.Empty,
                Type = type,
                Order = order,
                Options = new List<AnswerOptionDTO>(),
                CorrectOptionIds = new List<Guid>()
            },

            QuestionType.TrueFalse => CreateTrueFalseQuestion(id, order),

            QuestionType.OpenAnswer => new OpenAnswerQuestionDTO
            {
                Id = id,
                Text = string.Empty,
                Type = type,
                Order = order,
                CorrectAnswer = string.Empty
            },

            QuestionType.Matching => new MatchingQuestionDTO
            {
                Id = id,
                Text = string.Empty,
                Type = type,
                Order = order,
                Pairs = new List<MatchingPairDTO>()
            },

            QuestionType.Ordering => new OrderingQuestionDTO
            {
                Id = id,
                Text = string.Empty,
                Type = type,
                Order = order,
                Options = new List<AnswerOptionDTO>()
            },

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Неизвестный тип вопроса.")
        };
    }

    private static TrueFalseQuestionDTO CreateTrueFalseQuestion(Guid id, int order)
    {
        return new TrueFalseQuestionDTO
        {
            Id = id,
            Text = string.Empty,
            Type = QuestionType.TrueFalse,
            Order = order,
            Options = new List<AnswerOptionDTO>
            {
                new AnswerOptionDTO
                {
                    Id = Guid.NewGuid(),
                    Text = "Верно",
                    Order = 1
                },
                new AnswerOptionDTO
                {
                    Id = Guid.NewGuid(),
                    Text = "Неверно",
                    Order = 2
                }
            },
            CorrectOptionId = null
        };
    }
}