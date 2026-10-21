
using System;
using System.Collections.Generic;
using System.Linq;

public static class TestMapper
{
    public static TestDTO ToDto(Test test)
    {
        return new TestDTO
        {
            Id = test.Id,
            Title = test.Title,

            Questions = test.TestQuestions
                .OrderBy(tq => tq.Order)
                .Select(tq => ToQuestionDto(tq.Question, tq.Order))
                .ToList()
        };
    }

    public static BaseQuestionDTO ToQuestionDto(Question question, int order)
    {
        switch (question.Type)
        {
            case QuestionType.SingleChoice:
                return new SingleChoiceQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    Order = order,

                    Options = question.Options
                        .OrderBy(o => o.Order)
                        .Select(ToOptionDto)
                        .ToList(),

                    CorrectOptionId = question.CorrectAnswers
                        .Where(ca => ca.AnswerOptionId.HasValue)
                        .Select(ca => ca.AnswerOptionId.Value)
                        .FirstOrDefault()
                };

            case QuestionType.MultipleChoice:
                return new MultipleChoiceQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    Order = order,

                    Options = question.Options
                        .OrderBy(o => o.Order)
                        .Select(ToOptionDto)
                        .ToList(),

                    CorrectOptionIds = question.CorrectAnswers
                        .Where(ca => ca.AnswerOptionId.HasValue).Select(ca => ca.AnswerOptionId.Value).ToList()
                };

            case QuestionType.TrueFalse:
                return new TrueFalseQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    Order = order,

                    Options = question.Options
                        .OrderBy(o => o.Order)
                        .Select(ToOptionDto)
                        .ToList(),

                    CorrectOptionId = question.CorrectAnswers
                        .Where(ca => ca.AnswerOptionId.HasValue)
                        .Select(ca => ca.AnswerOptionId.Value)
                        .FirstOrDefault()
                };

            case QuestionType.OpenAnswer:
                return new OpenAnswerQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    Order = order,

                    CorrectAnswer = question.CorrectAnswers
                        .Where(ca => ca.Text != null)
                        .Select(ca => ca.Text)
                        .FirstOrDefault() ?? string.Empty
                };

            case QuestionType.Ordering:
                return new OrderingQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    Order = order,

                    Options = question.Options
                        .OrderBy(o => o.Order)
                        .Select(ToOptionDto)
                        .ToList()
                };

            case QuestionType.Matching:
                return new MatchingQuestionDTO
                {
                    Id = question.Id,
                    Text = question.Text,
                    Type = question.Type,
                    Order = order,

                    Pairs = question.MatchingPairs
                        .Select(pair => new MatchingPairDTO
                        {
                            Id = pair.Id,
                            LeftText = pair.LeftText,
                            RightText = pair.RightText
                        })
                        .ToList()
                };

            default:
                throw new NotSupportedException(
                    $"Question type {question.Type} is not supported.");
        }
    }

    private static AnswerOptionDTO ToOptionDto(AnswerOption option)
    {
        return new AnswerOptionDTO
        {
            Id = option.Id,
            Text = option.Text,
            Order = option.Order
        };
    }

    public static void UpdateQuestionEntity(BaseQuestionDTO source, Question destination)
    {
        destination.Text = source.Text;
        destination.Type = source.Type;

        switch (source)
        {
            case SingleChoiceQuestionDTO dto:
                UpdateOptions(destination, dto.Options);
                ReplaceCorrectAnswers(destination, new[] { dto.CorrectOptionId });
                break;

            case MultipleChoiceQuestionDTO dto:
                UpdateOptions(destination, dto.Options);
                ReplaceCorrectAnswers(
                    destination,
                    dto.CorrectOptionIds.Select(id => (Guid?)id));
                break;

            case TrueFalseQuestionDTO dto:
                UpdateOptions(destination, dto.Options);
                ReplaceCorrectAnswers(destination, new[] { dto.CorrectOptionId });
                break;

            case OpenAnswerQuestionDTO dto:
                destination.Options.Clear();

                var textAnswer = destination.CorrectAnswers
                    .FirstOrDefault(answer => answer.AnswerOptionId == null);

                if (textAnswer == null)
                {
                    textAnswer = new CorrectAnswer
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = destination.Id,
                        Question = destination
                    };

                    destination.CorrectAnswers.Add(textAnswer);
                }

                textAnswer.AnswerOptionId = null;
                textAnswer.Text = dto.CorrectAnswer;

                foreach (var answer in destination.CorrectAnswers
                    .Where(answer => answer.Id != textAnswer.Id)
                    .ToList())
                {
                    destination.CorrectAnswers.Remove(answer);
                }

                break;

            case OrderingQuestionDTO dto:
                UpdateOptions(destination, dto.Options);

                foreach (var answer in destination.CorrectAnswers.ToList())
                {
                    destination.CorrectAnswers.Remove(answer);
                }

                break;

            case MatchingQuestionDTO dto:
                foreach (var option in destination.Options.ToList())
                {
                    destination.Options.Remove(option);
                }

                foreach (var answer in destination.CorrectAnswers.ToList())
                {
                    destination.CorrectAnswers.Remove(answer);
                }

                UpdateMatchingPairs(destination, dto.Pairs);
                break;

            default:
                throw new NotSupportedException(
                    $"Question type {source.Type} is not supported.");
        }
    }

    private static void UpdateOptions(
        Question question,
        IEnumerable<AnswerOptionDTO> sourceOptions)
    {
        var options = (sourceOptions ?? Enumerable.Empty<AnswerOptionDTO>()).ToList();

        foreach (var dto in options)
        {
            if (dto.Id == Guid.Empty)
                dto.Id = Guid.NewGuid();
        }

        var sourceIds = options.Select(o => o.Id).ToHashSet();

        foreach (var existing in question.Options
            .Where(o => !sourceIds.Contains(o.Id)).ToList())
        {
            question.Options.Remove(existing);
        }

        foreach (var dto in options)
        {
            var option = question.Options.FirstOrDefault(o => o.Id == dto.Id);

            if (option == null)
            {
                option = new AnswerOption
                {
                    Id = dto.Id,
                    QuestionId = question.Id,
                    Question = question,
                    Text = dto.Text,
                    Order = dto.Order
                };

                question.Options.Add(option);
            }
            else
            {
                option.Text = dto.Text;
                option.Order = dto.Order;
            }
        }
    }

    private static void ReplaceCorrectAnswers(
        Question question,
        IEnumerable<Guid?> correctOptionIds)
    {
        var desiredIds = (correctOptionIds ?? Enumerable.Empty<Guid?>())
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToHashSet();

        foreach (Guid optionId in desiredIds)
        {
            if (!question.Options.Any(option => option.Id == optionId))
            {
                throw new InvalidOperationException(
                    $"Правильный вариант {optionId} не найден среди вариантов вопроса.");
            }
        }

        var existingAnswers = question.CorrectAnswers
            .Where(answer => answer.AnswerOptionId.HasValue)
            .ToList();

        foreach (var answer in existingAnswers)
        {
            if (!desiredIds.Contains(answer.AnswerOptionId!.Value))
            {
                question.CorrectAnswers.Remove(answer);
            }
            else
            {
                desiredIds.Remove(answer.AnswerOptionId.Value);
            }
        }

        foreach (Guid optionId in desiredIds)
        {
            question.CorrectAnswers.Add(new CorrectAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Question = question,
                AnswerOptionId = optionId
            });
        }

        foreach (var answer in question.CorrectAnswers
            .Where(answer => answer.Text != null)
            .ToList())
        {
            question.CorrectAnswers.Remove(answer);
        }
    }

    private static void UpdateMatchingPairs(
        Question question,
        IEnumerable<MatchingPairDTO> sourcePairs)
    {
        var pairs = (sourcePairs ?? Enumerable.Empty<MatchingPairDTO>()).ToList();

        foreach (var dto in pairs)
        {
            if (dto.Id == Guid.Empty)
                dto.Id = Guid.NewGuid();
        }

        var sourceIds = pairs.Select(p => p.Id).ToHashSet();

        foreach (var existing in question.MatchingPairs
            .Where(p => !sourceIds.Contains(p.Id)).ToList())
        {
            question.MatchingPairs.Remove(existing);
        }

        foreach (var dto in pairs)
        {
            var pair = question.MatchingPairs.FirstOrDefault(p => p.Id == dto.Id);

            if (pair == null)
            {
                pair = new MatchingPair
                {
                    Id = dto.Id,
                    QuestionId = question.Id,
                    Question = question
                };

                question.MatchingPairs.Add(pair);
            }

            pair.LeftText = dto.LeftText;
            pair.RightText = dto.RightText;
        }
    }
}
