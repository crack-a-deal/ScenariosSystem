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