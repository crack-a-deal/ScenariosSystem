using System;
using System.Collections.Generic;
using System.Linq;

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
    public static BaseQuestionDTO Clone(BaseQuestionDTO source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return source switch
        {
            SingleChoiceQuestionDTO question => CloneSingleChoice(question),
            MultipleChoiceQuestionDTO question => CloneMultipleChoice(question),
            TrueFalseQuestionDTO question => CloneTrueFalse(question),
            OpenAnswerQuestionDTO question => new OpenAnswerQuestionDTO
            {
                Id = Guid.NewGuid(),
                Text = question.Text,
                Type = question.Type,
                Order = question.Order,
                CorrectAnswer = question.CorrectAnswer
            },
            OrderingQuestionDTO question => new OrderingQuestionDTO
            {
                Id = Guid.NewGuid(),
                Text = question.Text,
                Type = question.Type,
                Order = question.Order,
                Options = CloneOptions(question.Options, out _)
            },
            MatchingQuestionDTO question => new MatchingQuestionDTO
            {
                Id = Guid.NewGuid(),
                Text = question.Text,
                Type = question.Type,
                Order = question.Order,
                Pairs = (question.Pairs ?? new List<MatchingPairDTO>())
                    .Select(pair => new MatchingPairDTO
                    {
                        Id = Guid.NewGuid(),
                        LeftText = pair.LeftText,
                        RightText = pair.RightText
                    })
                    .ToList()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестный тип вопроса.")
        };
    }

    private static List<AnswerOptionDTO> CloneOptions(
        List<AnswerOptionDTO> options,
        out Dictionary<Guid, Guid> idByOldId)
    {
        idByOldId = new Dictionary<Guid, Guid>();
        List<AnswerOptionDTO> clones = new();

        foreach (AnswerOptionDTO option in options ?? new List<AnswerOptionDTO>())
        {
            Guid newId = Guid.NewGuid();

            idByOldId[option.Id] = newId;
            clones.Add(new AnswerOptionDTO
            {
                Id = newId,
                Text = option.Text,
                Order = option.Order
            });
        }

        return clones;
    }

    private static SingleChoiceQuestionDTO CloneSingleChoice(SingleChoiceQuestionDTO question)
    {
        List<AnswerOptionDTO> options =
            CloneOptions(question.Options, out Dictionary<Guid, Guid> idByOldId);

        return new SingleChoiceQuestionDTO
        {
            Id = Guid.NewGuid(),
            Text = question.Text,
            Type = question.Type,
            Order = question.Order,
            Options = options,
            CorrectOptionId =
                question.CorrectOptionId.HasValue && idByOldId.ContainsKey(question.CorrectOptionId.Value)
                    ? idByOldId[question.CorrectOptionId.Value]
                    : question.CorrectOptionId
        };
    }

    private static MultipleChoiceQuestionDTO CloneMultipleChoice(MultipleChoiceQuestionDTO question)
    {
        List<AnswerOptionDTO> options =
            CloneOptions(question.Options, out Dictionary<Guid, Guid> idByOldId);

        return new MultipleChoiceQuestionDTO
        {
            Id = Guid.NewGuid(),
            Text = question.Text,
            Type = question.Type,
            Order = question.Order,
            Options = options,
            CorrectOptionIds = (question.CorrectOptionIds ?? new List<Guid>())
                .Where(idByOldId.ContainsKey)
                .Select(id => idByOldId[id])
                .ToList()
        };
    }

    private static TrueFalseQuestionDTO CloneTrueFalse(TrueFalseQuestionDTO question)
    {
        List<AnswerOptionDTO> options =
            CloneOptions(question.Options, out Dictionary<Guid, Guid> idByOldId);

        return new TrueFalseQuestionDTO
        {
            Id = Guid.NewGuid(),
            Text = question.Text,
            Type = question.Type,
            Order = question.Order,
            Options = options,
            CorrectOptionId =
                question.CorrectOptionId.HasValue && idByOldId.ContainsKey(question.CorrectOptionId.Value)
                    ? idByOldId[question.CorrectOptionId.Value]
                    : question.CorrectOptionId
        };
    }
}