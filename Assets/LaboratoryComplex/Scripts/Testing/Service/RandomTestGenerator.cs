
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class RandomTestGenerator
{
    private readonly AppDbContext _context;

    public RandomTestGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Test> CreateAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Название теста не может быть пустым.",
                nameof(title));
        }

        // TEST ONLY
        //Dictionary<QuestionType, int> requiredCounts =
        //    new Dictionary<QuestionType, int>
        //    {
        //                { QuestionType.OpenAnswer, 1 },
        //                { QuestionType.SingleChoice, 1 },
        //                { QuestionType.MultipleChoice, 1 },
        //                { QuestionType.Matching, 1 },
        //                { QuestionType.Ordering, 1 },
        //                { QuestionType.TrueFalse, 1 }
        //    };

        Dictionary<QuestionType, int> requiredCounts =
            new Dictionary<QuestionType, int>
            {
                { QuestionType.OpenAnswer, 2 },
                { QuestionType.SingleChoice, 6 },
                { QuestionType.MultipleChoice, 4 },
                { QuestionType.Matching, 3 },
                { QuestionType.Ordering, 2 },
                { QuestionType.TrueFalse, 3 }
            };

        List<Question> questions = await _context.Questions
            .AsNoTracking()
            .ToListAsync();

        if (questions.Count == 0)
        {
            throw new InvalidOperationException(
                "В базе данных нет вопросов.");
        }

        foreach (KeyValuePair<QuestionType, int> item in requiredCounts)
        {
            int availableCount = questions.Count(
                q => q.Type == item.Key);

            if (availableCount < item.Value)
            {
                throw new InvalidOperationException(
                    $"Для типа '{item.Key}' требуется " +
                    $"{item.Value} вопросов, а в базе есть " +
                    $"{availableCount}.");
            }
        }

        Random random = new Random();

        List<Question> selectedQuestions = new List<Question>();

        foreach (KeyValuePair<QuestionType, int> item in requiredCounts)
        {
            List<Question> selectedByType = questions
                .Where(q => q.Type == item.Key)
                .OrderBy(q => random.Next())
                .Take(item.Value)
                .ToList();

            selectedQuestions.AddRange(selectedByType);
        }

        selectedQuestions = selectedQuestions
            .OrderBy(q => random.Next())
            .ToList();

        Test test = new Test
        {
            Id = Guid.NewGuid(),
            Title = title
        };

        for (int i = 0; i < selectedQuestions.Count; i++)
        {
            test.TestQuestions.Add(new TestQuestion
            {
                TestId = test.Id,
                QuestionId = selectedQuestions[i].Id,
                Order = i + 1
            });
        }

        _context.Tests.Add(test);

        await _context.SaveChangesAsync();

        // Возвращаем тест вместе с вопросами и их содержимым,
        // чтобы TestMapper мог сформировать DTO.
        Test? savedTest = await _context.Tests
            .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                    .ThenInclude(q => q.Options)

            .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                    .ThenInclude(q => q.CorrectAnswers)
                        .ThenInclude(ca => ca.AnswerOption)

            .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                    .ThenInclude(q => q.MatchingPairs)

            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == test.Id);

        if (savedTest == null)
        {
            throw new InvalidOperationException(
                "Не удалось загрузить созданный тест.");
        }

        return savedTest;
    }
}
