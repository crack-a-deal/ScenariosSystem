using System;
using System.Linq;

public class TestSessionResultExcelMapper
{
    public TestSessionResultExcelDto Map(
    BaseQuestionDTO question,
    UserBaseAnswerDTO answer)
    {
        return new TestSessionResultExcelDto
        {
            QuestionNumber = question.Order,
            QuestionText = question.Text,
            QuestionType = GetQuestionTypeName(question.Type),

            UserAnswer = GetUserAnswer(
                question,
                answer),

            CorrectAnswer = GetCorrectAnswer(
                question),

            Result = GetResult(
                question,
                answer)
        };
    }

    private string GetQuestionTypeName(
        QuestionType type)
    {
        return type switch
        {
            QuestionType.SingleChoice =>
                "Один вариант",

            QuestionType.MultipleChoice =>
                "Несколько вариантов",

            QuestionType.TrueFalse =>
                "Да / Нет",

            QuestionType.OpenAnswer =>
                "Открытый ответ",

            QuestionType.Ordering =>
                "Порядок",

            QuestionType.Matching =>
                "Сопоставление",

            _ =>
                type.ToString()
        };
    }

    private string GetUserAnswer(
    BaseQuestionDTO question,
    UserBaseAnswerDTO answer)
    {
        if (answer == null)
            return "Нет ответа";

        if (question is SingleChoiceQuestionDTO singleChoice &&
            answer is UserSingleChoiceAnswerDTO singleChoiceAnswer)
        {
            var selectedOption =
                singleChoice.Options.FirstOrDefault(
                    option => option.Id == singleChoiceAnswer.SelectedOptionId);

            return selectedOption?.Text ?? "Нет ответа";
        }

        if (question is MultipleChoiceQuestionDTO multipleChoice &&
            answer is UserMultipleChoiceAnswerDTO multipleChoiceAnswer)
        {
            var selectedOptions =
                multipleChoice.Options
                    .Where(option =>
                        multipleChoiceAnswer.SelectedOptionIds
                            .Contains(option.Id))
                    .Select(option => option.Text);

            return string.Join(", ", selectedOptions);
        }

        if (question is TrueFalseQuestionDTO trueFalse &&
            answer is UserTrueFalseAnswerDTO trueFalseAnswer)
        {
            var selectedOption =
                trueFalse.Options.FirstOrDefault(
                    option => option.Id == trueFalseAnswer.SelectedOptionId);

            return selectedOption?.Text ?? "Нет ответа";
        }

        if (question is OpenAnswerQuestionDTO &&
            answer is UserOpenAnswerDTO openAnswer)
        {
            return string.IsNullOrWhiteSpace(openAnswer.Text)
                ? "Нет ответа"
                : openAnswer.Text;
        }

        if (question is OrderingQuestionDTO ordering &&
            answer is UserOrderingAnswerDTO orderingAnswer)
        {
            var selectedOptions =
                orderingAnswer.Items
                    .OrderBy(item => item.Order)
                    .Select(item =>
                        ordering.Options
                            .FirstOrDefault(option =>
                                option.Id == item.OptionId)?.Text)
                    .Where(text => !string.IsNullOrEmpty(text));

            return string.Join(" → ", selectedOptions);
        }

        if (question is MatchingQuestionDTO matching &&
            answer is UserMatchingAnswerDTO matchingAnswer)
        {
            var userPairs =
                matchingAnswer.Pairs
                    .Select(pair =>
                    {
                        var leftPair =
                            matching.Pairs.FirstOrDefault(
                                p => p.Id == pair.LeftPairId);

                        var rightPair =
                            matching.Pairs.FirstOrDefault(
                                p => p.Id == pair.RightPairId);

                        if (leftPair == null || rightPair == null)
                            return null;

                        return $"{leftPair.LeftText} → {rightPair.RightText}";
                    })
                    .Where(text => !string.IsNullOrEmpty(text));

            return string.Join(", ", userPairs);
        }

        return "";
    }

    private string GetCorrectAnswer(
    BaseQuestionDTO question)
    {
        if (question is SingleChoiceQuestionDTO singleChoice)
        {
            var correctOption =
                singleChoice.Options.FirstOrDefault(
                    option => option.Id == singleChoice.CorrectOptionId);

            return correctOption?.Text ?? "Не указан";
        }

        if (question is MultipleChoiceQuestionDTO multipleChoice)
        {
            var correctOptions =
                multipleChoice.Options
                    .Where(option =>
                        multipleChoice.CorrectOptionIds
                            .Contains(option.Id))
                    .Select(option => option.Text);

            var result = string.Join(", ", correctOptions);

            return string.IsNullOrEmpty(result)
                ? "Не указан"
                : result;
        }

        if (question is TrueFalseQuestionDTO trueFalse)
        {
            var correctOption =
                trueFalse.Options.FirstOrDefault(
                    option => option.Id == trueFalse.CorrectOptionId);

            return correctOption?.Text ?? "Не указан";
        }

        if (question is OpenAnswerQuestionDTO openAnswer)
        {
            return string.IsNullOrWhiteSpace(openAnswer.CorrectAnswer)
                ? "Не указан"
                : openAnswer.CorrectAnswer;
        }

        if (question is OrderingQuestionDTO ordering)
        {
            var correctOptions =
                ordering.Options
                    .OrderBy(option => option.Order)
                    .Select(option => option.Text);

            return string.Join(" → ", correctOptions);
        }

        if (question is MatchingQuestionDTO matching)
        {
            var correctPairs =
                matching.Pairs
                    .Select(pair =>
                        $"{pair.LeftText} → {pair.RightText}");

            return string.Join(", ", correctPairs);
        }

        return "";
    }

    private string GetResult(
    BaseQuestionDTO question,
    UserBaseAnswerDTO answer)
    {
        if (question is SingleChoiceQuestionDTO singleChoice &&
            answer is UserSingleChoiceAnswerDTO singleChoiceAnswer)
        {
            return singleChoice.CorrectOptionId ==
                   singleChoiceAnswer.SelectedOptionId
                ? "Верно"
                : "Неверно";
        }

        if (question is MultipleChoiceQuestionDTO multipleChoice &&
            answer is UserMultipleChoiceAnswerDTO multipleChoiceAnswer)
        {
            var correctIds =
                multipleChoice.CorrectOptionIds;

            var selectedIds =
                multipleChoiceAnswer.SelectedOptionIds;

            bool isCorrect =
                correctIds.Count == selectedIds.Count &&
                correctIds.All(
                    id => selectedIds.Contains(id));

            return isCorrect
                ? "Верно"
                : "Неверно";
        }

        if (question is TrueFalseQuestionDTO trueFalse &&
            answer is UserTrueFalseAnswerDTO trueFalseAnswer)
        {
            return trueFalse.CorrectOptionId ==
                   trueFalseAnswer.SelectedOptionId
                ? "Верно"
                : "Неверно";
        }

        if (question is OpenAnswerQuestionDTO openAnswer &&
            answer is UserOpenAnswerDTO userOpenAnswer)
        {
            if (string.IsNullOrWhiteSpace(userOpenAnswer.Text))
                return "";

            string userText =
                userOpenAnswer.Text.Trim();

            string correctText =
                openAnswer.CorrectAnswer.Trim();

            return string.Equals(
                userText,
                correctText,
                System.StringComparison.OrdinalIgnoreCase)
                ? "Верно"
                : "Неверно";
        }

        if (question is OrderingQuestionDTO ordering &&
            answer is UserOrderingAnswerDTO orderingAnswer)
        {
            var correctIds =
                ordering.Options
                    .OrderBy(option => option.Order)
                    .Select(option => option.Id)
                    .ToList();

            var selectedIds =
                orderingAnswer.Items
                    .OrderBy(item => item.Order)
                    .Select(item => item.OptionId)
                    .ToList();

            bool isCorrect =
                correctIds.Count == selectedIds.Count &&
                correctIds.SequenceEqual(selectedIds);

            return isCorrect
                ? "Верно"
                : "Неверно";
        }

        if (question is MatchingQuestionDTO matching &&
            answer is UserMatchingAnswerDTO matchingAnswer)
        {
            bool isCorrect =
                matchingAnswer.Pairs.Count == matching.Pairs.Count &&
                matchingAnswer.Pairs.All(
                    pair => pair.LeftPairId == pair.RightPairId);

            return isCorrect
                ? "Верно"
                : "Неверно";
        }

        return "";
    }
}