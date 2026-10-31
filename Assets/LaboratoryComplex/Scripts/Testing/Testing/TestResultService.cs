using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class TestResultService
{
    private readonly AppDbContext _context;

    public TestResultService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(int CorrectAnswers, int TotalQuestions)> CalculateAsync(
        Guid sessionId,
        List<UserBaseAnswerDTO> answers)
    {
        var session = await _context.TestSessions
            .Include(s => s.Test)
                .ThenInclude(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.Options)
            .Include(s => s.Test)
                .ThenInclude(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.CorrectAnswers)
            .Include(s => s.Test)
                .ThenInclude(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.MatchingPairs)
            .SingleOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new InvalidOperationException("Сессия теста не найдена.");

        var questions = session.Test.TestQuestions
            .Select(tq => tq.Question)
            .ToList();

        var answersByQuestion = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.First());

        int correctAnswers = 0;

        foreach (var question in questions)
        {
            if (!answersByQuestion.TryGetValue(question.Id, out var answer))
                continue;

            bool isCorrect = question.Type switch
            {
                QuestionType.SingleChoice =>
                    IsSingleChoiceCorrect(question.CorrectAnswers, answer),

                QuestionType.TrueFalse =>
                    IsSingleChoiceCorrect(question.CorrectAnswers, answer),

                QuestionType.MultipleChoice =>
                    IsMultipleChoiceCorrect(question.CorrectAnswers, answer),

                QuestionType.Ordering =>
                    IsOrderingCorrect(question.Options, answer),

                QuestionType.Matching =>
                    IsMatchingCorrect(question.MatchingPairs, answer),

                QuestionType.OpenAnswer =>
    IsOpenAnswerCorrect(question.CorrectAnswers, answer),

                _ => false
            };

            if (isCorrect)
                correctAnswers++;
        }

        return (correctAnswers, questions.Count);
    }

    private static bool IsSingleChoiceCorrect(
        IEnumerable<CorrectAnswer> correctAnswers,
        UserBaseAnswerDTO answer)
    {
        Guid? selectedOptionId = answer switch
        {
            UserSingleChoiceAnswerDTO single => single.SelectedOptionId,
            UserTrueFalseAnswerDTO trueFalse => trueFalse.SelectedOptionId,
            _ => null
        };

        var correctIds = correctAnswers
            .Where(a => a.AnswerOptionId.HasValue)
            .Select(a => a.AnswerOptionId!.Value)
            .Distinct()
            .ToList();

        return selectedOptionId.HasValue
            && correctIds.Count == 1
            && correctIds[0] == selectedOptionId.Value;
    }

    private static bool IsMultipleChoiceCorrect(
        IEnumerable<CorrectAnswer> correctAnswers,
        UserBaseAnswerDTO answer)
    {
        if (answer is not UserMultipleChoiceAnswerDTO multiple)
            return false;

        var expected = correctAnswers
            .Where(a => a.AnswerOptionId.HasValue)
            .Select(a => a.AnswerOptionId!.Value)
            .ToHashSet();

        var selected = multiple.SelectedOptionIds.ToHashSet();

        return expected.Count > 0 && expected.SetEquals(selected);
    }

    private static bool IsOrderingCorrect(
        IEnumerable<AnswerOption> options,
        UserBaseAnswerDTO answer)
    {
        if (answer is not UserOrderingAnswerDTO ordering)
            return false;

        // Предполагается, что AnswerOption.Order задаёт
        // правильную последовательность.
        var expected = options
            .OrderBy(o => o.Order)
            .Select(o => o.Id)
            .ToList();

        var actual = ordering.Items
            .OrderBy(item => item.Order)
            .Select(item => item.OptionId)
            .ToList();

        return expected.Count > 0
            && expected.SequenceEqual(actual);
    }

    private static bool IsMatchingCorrect(
        IEnumerable<MatchingPair> matchingPairs,
        UserBaseAnswerDTO answer)
    {
        if (answer is not UserMatchingAnswerDTO matching)
            return false;

        var expectedIds = matchingPairs
            .Select(pair => pair.Id)
            .ToHashSet();

        if (expectedIds.Count == 0 || matching.Pairs.Count != expectedIds.Count)
            return false;

        // Предполагается, что правильная пара задаётся
        // одинаковым MatchingPair.Id слева и справа.
        var actualIds = new HashSet<Guid>();

        foreach (var pair in matching.Pairs)
        {
            if (pair.LeftPairId != pair.RightPairId)
                return false;

            if (!expectedIds.Contains(pair.LeftPairId))
                return false;

            if (!actualIds.Add(pair.LeftPairId))
                return false;
        }

        return expectedIds.SetEquals(actualIds);
    }

    private static bool IsOpenAnswerCorrect(
    IEnumerable<CorrectAnswer> correctAnswers,
    UserBaseAnswerDTO answer)
    {
        if (answer is not UserOpenAnswerDTO open)
            return false;

        if (string.IsNullOrWhiteSpace(open.Text))
            return false;

        string expected = correctAnswers
            .Select(correct => correct.Text)
            .FirstOrDefault(text =>
                !string.IsNullOrWhiteSpace(text));

        if (expected == null)
            return false;

        return string.Equals(
            open.Text.Trim(),
            expected.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}