using System;
using System.Collections.Generic;
using System.Linq;

public static class UserAnswerMapper
{
    public static UserAnswer ToEntity(
    UserBaseAnswerDTO dto,
    Guid sessionId)
    {
        var answer = new UserAnswer
        {
            TestSessionId = sessionId,
            QuestionId = dto.QuestionId
        };

        switch (dto)
        {
            case UserSingleChoiceAnswerDTO:
                break;

            case UserMultipleChoiceAnswerDTO:
                break;

            case UserTrueFalseAnswerDTO:
                break;

            case UserOpenAnswerDTO openAnswer:
                answer.Text = openAnswer.Text;
                break;

            case UserOrderingAnswerDTO:
                break;

            case UserMatchingAnswerDTO:
                break;
        }

        return answer;
    }

    public static UserAnswerOption ToOptionEntity(
    UserSingleChoiceAnswerDTO dto,
    Guid userAnswerId)
    {
        return new UserAnswerOption
        {
            UserAnswerId = userAnswerId,
            AnswerOptionId = dto.SelectedOptionId!.Value
        };
    }

    public static List<UserAnswerOption> ToOptionEntities(
    UserMultipleChoiceAnswerDTO dto,
    Guid userAnswerId)
    {
        return dto.SelectedOptionIds
            .Select(optionId => new UserAnswerOption
            {
                UserAnswerId = userAnswerId,
                AnswerOptionId = optionId
            })
            .ToList();
    }

    public static UserAnswerOption ToOptionEntity(
    UserTrueFalseAnswerDTO dto,
    Guid userAnswerId)
    {
        return new UserAnswerOption
        {
            UserAnswerId = userAnswerId,
            AnswerOptionId = dto.SelectedOptionId!.Value
        };
    }

    public static List<UserAnswerOption> ToOptionEntities(
    UserOrderingAnswerDTO dto,
    Guid userAnswerId)
    {
        return dto.Items
            .Select(item => new UserAnswerOption
            {
                UserAnswerId = userAnswerId,
                AnswerOptionId = item.OptionId,
                Order = item.Order
            })
            .ToList();
    }

    public static List<UserMatchingPairAnswer> ToMatchingPairEntities(
    UserMatchingAnswerDTO dto,
    Guid userAnswerId)
    {
        return dto.Pairs
            .Select(pair => new UserMatchingPairAnswer
            {
                UserAnswerId = userAnswerId,
                LeftPairId = pair.LeftPairId,
                RightPairId = pair.RightPairId
            })
            .ToList();
    }
}

public static class UserAnswerExportMapper
{
    public static TestSessionDTO ToSessionDto(
        TestSession session,
        TestDTO test)
    {
        var questionsById = test.Questions
            .ToDictionary(question => question.Id);

        var answers = new List<UserBaseAnswerDTO>();

        foreach (var answer in session.Answers)
        {
            if (!questionsById.TryGetValue(
                    answer.QuestionId,
                    out var question))
            {
                continue;
            }

            UserBaseAnswerDTO dto = question switch
            {
                SingleChoiceQuestionDTO => MapSingleChoice(answer),
                TrueFalseQuestionDTO => MapTrueFalse(answer),
                MultipleChoiceQuestionDTO => MapMultipleChoice(answer),
                OrderingQuestionDTO => MapOrdering(answer),
                MatchingQuestionDTO => MapMatching(answer),
                OpenAnswerQuestionDTO => MapOpenAnswer(answer),
                _ => null
            };

            if (dto != null)
                answers.Add(dto);
        }

        return new TestSessionDTO
        {
            Answers = answers
        };
    }

    private static UserSingleChoiceAnswerDTO MapSingleChoice(UserAnswer answer)
    {
        return new UserSingleChoiceAnswerDTO
        {
            QuestionId = answer.QuestionId,
            SelectedOptionId = answer.Options
                .Select(option => (Guid?)option.AnswerOptionId)
                .FirstOrDefault()
        };
    }

    private static UserTrueFalseAnswerDTO MapTrueFalse(UserAnswer answer)
    {
        return new UserTrueFalseAnswerDTO
        {
            QuestionId = answer.QuestionId,
            SelectedOptionId = answer.Options
                .Select(option => (Guid?)option.AnswerOptionId)
                .FirstOrDefault()
        };
    }

    private static UserMultipleChoiceAnswerDTO MapMultipleChoice(UserAnswer answer)
    {
        return new UserMultipleChoiceAnswerDTO
        {
            QuestionId = answer.QuestionId,
            SelectedOptionIds = answer.Options
                .Select(option => option.AnswerOptionId)
                .ToList()
        };
    }

    private static UserOrderingAnswerDTO MapOrdering(UserAnswer answer)
    {
        return new UserOrderingAnswerDTO
        {
            QuestionId = answer.QuestionId,
            Items = answer.Options
                .Select((option, index) => new UserOrderingItemDTO
                {
                    OptionId = option.AnswerOptionId,
                    Order = option.Order ?? index
                })
                .ToList()
        };
    }

    private static UserMatchingAnswerDTO MapMatching(UserAnswer answer)
    {
        return new UserMatchingAnswerDTO
        {
            QuestionId = answer.QuestionId,
            Pairs = answer.MatchingPairs
                .Select(pair => new UserMatchingPairAnswerDTO
                {
                    LeftPairId = pair.LeftPairId,
                    RightPairId = pair.RightPairId
                })
                .ToList()
        };
    }

    private static UserOpenAnswerDTO MapOpenAnswer(UserAnswer answer)
    {
        return new UserOpenAnswerDTO
        {
            QuestionId = answer.QuestionId,
            Text = answer.Text ?? string.Empty
        };
    }
}