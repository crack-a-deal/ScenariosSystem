using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TestRepository
{
    private readonly AppDbContext _context;

    public TestRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<Test?> GetFirstWithQuestionsAsync()
    {
        return await _context.Tests
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
            .FirstOrDefaultAsync();
    }



    public async Task<Test?> GetByIdWithQuestionsAsync(Guid testId)
    {
        return await _context.Tests
            .Include(t => t.TestQuestions
                .OrderBy(tq => tq.Order))
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
            .FirstOrDefaultAsync(t => t.Id == testId);
    }

    public async Task<List<Test>> GetAllAsync()
    {
        return await _context.Tests
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
            .OrderBy(t => t.Title)
            .ToListAsync();
    }
    public async Task DeleteAsync(Guid testId)
    {
        var test = await _context.Tests
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null)
            return;

        _context.Tests.Remove(test);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateQuestionAsync(Guid testId,Guid oldQuestionId,Guid newQuestionId)
    {
        if (oldQuestionId == newQuestionId)
            return;

        var testQuestion = await _context.TestQuestions
            .Include(tq => tq.Question)
            .FirstOrDefaultAsync(tq =>
                tq.TestId == testId &&
                tq.QuestionId == oldQuestionId);

        if (testQuestion == null)
        {
            throw new InvalidOperationException(
                "Вопрос не найден в указанном тесте.");
        }

        var newQuestion = await _context.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == newQuestionId);

        if (newQuestion == null)
        {
            throw new InvalidOperationException(
                "Новый вопрос не найден.");
        }

        if (testQuestion.Question.Type != newQuestion.Type)
        {
            throw new InvalidOperationException(
                "Тип нового вопроса должен совпадать с типом заменяемого.");
        }

        bool alreadyExists = await _context.TestQuestions
            .AnyAsync(tq =>
                tq.TestId == testId &&
                tq.QuestionId == newQuestionId);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "Этот вопрос уже добавлен в тест.");
        }

        testQuestion.QuestionId = newQuestionId;

        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync(TestDTO testDto)
    {
        if (testDto == null)
            throw new ArgumentNullException(nameof(testDto));

        if (string.IsNullOrWhiteSpace(testDto.Title))
            throw new ArgumentException("Название теста не может быть пустым.", nameof(testDto));

        if (testDto.Questions == null || testDto.Questions.Count == 0)
            throw new ArgumentException("Тест должен содержать хотя бы один вопрос.", nameof(testDto));

        if (testDto.Id == Guid.Empty)
            testDto.Id = Guid.NewGuid();

        var questionIds = new HashSet<Guid>();

        foreach (var questionDto in testDto.Questions)
        {
            if (questionDto.Id == Guid.Empty)
                questionDto.Id = Guid.NewGuid();

            if (!questionIds.Add(questionDto.Id))
                throw new InvalidOperationException(
                    $"В тесте повторяется вопрос с ID {questionDto.Id}.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                .FirstOrDefaultAsync(t => t.Id == testDto.Id);

            bool isNewTest = test == null;

            if (isNewTest)
            {
                test = new Test
                {
                    Id = testDto.Id,
                    Title = testDto.Title
                };

                _context.Tests.Add(test);
            }
            else
            {
                test!.Title = testDto.Title;
            }

            // Удаляем связи с вопросами, которых больше нет в тесте.
            var removedLinks = test!.TestQuestions
                .Where(tq => !questionIds.Contains(tq.QuestionId))
                .ToList();

            var removedQuestionIds = removedLinks
                .Select(link => link.QuestionId)
                .Distinct()
                .ToList();

            foreach (var link in removedLinks)
            {
                test.TestQuestions.Remove(link);
                _context.TestQuestions.Remove(link);
            }

            foreach (var questionDto in testDto.Questions)
            {
                var question = await _context.Questions
                    .Include(q => q.Options)
                    .Include(q => q.CorrectAnswers)
                    .Include(q => q.MatchingPairs)
                    .FirstOrDefaultAsync(q => q.Id == questionDto.Id);

                bool isNewQuestion = question == null;

                // Запоминаем дочерние сущности, существовавшие до обновления.
                var existingOptionIds = question?.Options
                    .Select(option => option.Id)
                    .ToHashSet() ?? new HashSet<Guid>();

                var existingAnswerIds = question?.CorrectAnswers
                    .Select(answer => answer.Id)
                    .ToHashSet() ?? new HashSet<Guid>();

                var existingPairIds = question?.MatchingPairs
                    .Select(pair => pair.Id)
                    .ToHashSet() ?? new HashSet<Guid>();

                if (isNewQuestion)
                {
                    question = new Question
                    {
                        Id = questionDto.Id
                    };

                    _context.Questions.Add(question);
                }

                TestMapper.UpdateQuestionEntity(questionDto, question!);

                // Явно помечаем новые дочерние сущности как Added.
                // Это важно, поскольку новый вопрос уже мог быть добавлен
                // в контекст до наполнения его дочерних коллекций.
                foreach (var option in question!.Options)
                {
                    if (isNewQuestion || !existingOptionIds.Contains(option.Id))
                        _context.Entry(option).State = EntityState.Added;
                }

                foreach (var answer in question.CorrectAnswers)
                {
                    if (isNewQuestion || !existingAnswerIds.Contains(answer.Id))
                        _context.Entry(answer).State = EntityState.Added;
                }

                foreach (var pair in question.MatchingPairs)
                {
                    if (isNewQuestion || !existingPairIds.Contains(pair.Id))
                        _context.Entry(pair).State = EntityState.Added;
                }

                // Добавляем или обновляем связь вопроса с тестом.
                var link = test.TestQuestions
                    .FirstOrDefault(tq => tq.QuestionId == question.Id);

                if (link == null)
                {
                    link = new TestQuestion
                    {
                        TestId = test.Id,
                        Test = test,
                        QuestionId = question.Id,
                        Question = question,
                        Order = questionDto.Order
                    };

                    test.TestQuestions.Add(link);
                }
                else
                {
                    link.Order = questionDto.Order;
                }
            }

            await _context.SaveChangesAsync();

            // Удаляем вопросы, которые больше не используются другими тестами.
            foreach (var removedQuestionId in removedQuestionIds)
            {
                bool usedByAnotherTest = await _context.TestQuestions
                    .AnyAsync(tq =>
                        tq.QuestionId == removedQuestionId &&
                        tq.TestId != test.Id);

                if (usedByAnotherTest)
                    continue;

                var question = await _context.Questions
                    .FirstOrDefaultAsync(q => q.Id == removedQuestionId);

                if (question != null)
                    _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}