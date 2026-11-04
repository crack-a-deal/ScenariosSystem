using GroupModel;
using NUnit.Framework;
using PasswordHasher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAccountModel;
using UserProfileModel;

public sealed class DbTestScope : IDisposable
{
    public string DbPath { get; }
    public AppDbContext Context { get; }

    public TestSessionRepository Sessions { get; }
    public TestSessionService SessionService { get; }
    public TestResultService ResultService { get; }
    public UserAnswerSaveService AnswerSaver { get; }
    public ScenarioRepository Scenarios { get; }
    public ScenarioService ScenarioService { get; }
    public UserProfileRepository ProfileRepo { get; }
    public UserProfileService ProfileService { get; }
    public UserAccountRepository AccountRepo { get; }
    public UserAccountService AccountService { get; }
    public UserGroupRepository GroupRepo { get; }
    public PasswordHasherService Hasher { get; } = new PasswordHasherService();

    /// <summary>Группа по умолчанию для студентов (создаётся в CreateAsync).</summary>
    public UserGroup DefaultGroup { get; private set; } = null!;

    private DbTestScope(string dbPath)
    {
        DbPath = dbPath;
        Context = new AppDbContext(dbPath);

        Sessions = new TestSessionRepository(Context);
        SessionService = new TestSessionService(Context, Sessions);
        ResultService = new TestResultService(Context);

        AnswerSaver = new UserAnswerSaveService(
            new UserAnswerService(new UserAnswerRepository(Context)),
            new UserAnswerOptionService(new UserAnswerOptionRepository(Context)),
            new UserMatchingPairAnswerService(new UserMatchingPairAnswerRepository(Context)),
            Context);

        Scenarios = new ScenarioRepository(Context);
        ScenarioService = new ScenarioService(Scenarios);

        ProfileRepo = new UserProfileRepository(Context);
        GroupRepo = new UserGroupRepository(Context);
        AccountRepo = new UserAccountRepository(Context);
        ProfileService = new UserProfileService(ProfileRepo, AccountRepo, GroupRepo, Hasher);
        AccountService = new UserAccountService(AccountRepo, Hasher);
    }

    public static async Task<DbTestScope> CreateAsync()
    {
        var dbPath = Path.Combine(
            Path.GetTempPath(),
            $"scenariossystem-tests-{Guid.NewGuid():N}.db");

        var scope = new DbTestScope(dbPath);
        await scope.Context.Database.EnsureCreatedAsync();
        scope.DefaultGroup = await scope.CreateGroupAsync("Группа 101", 101);
        return scope;
    }

    /// <summary>
    /// Свежий контекст на тот же файл — для проверки персистентности
    /// (не зависит от кеша трекинга основного контекста фикстуры).
    /// </summary>
    public AppDbContext CreateReader() => new AppDbContext(DbPath);

    public void Dispose()
    {
        Context.Dispose();
        try { File.Delete(DbPath); } catch { /* файл может быть занят — не роняем teardown */ }
    }

    // --------------------------------------------------------------------------
    //  Сид-хелперы
    // --------------------------------------------------------------------------

    public async Task<UserGroup> CreateGroupAsync(string name, int number)
    {
        var group = new UserGroup { Id = Guid.NewGuid(), Name = name, Number = number };
        Context.UserGroups.Add(group);
        await Context.SaveChangesAsync();
        return group;
    }

    public Task<UserProfile> CreateProfileAsync(
        string firstName,
        string lastName,
        Guid? groupId = null,
        string? login = null)
    {
        return CreateProfileAsync(firstName, lastName, null, groupId, login);
    }

    public async Task<UserProfile> CreateProfileAsync(
        string firstName,
        string lastName,
        string? patronymic,
        Guid? groupId,
        string? login)
    {
        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            Login = login ?? $"student-{Guid.NewGuid():N}",
            PasswordHash = Hasher.Hash("test-password")
        };

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserAccountId = account.Id,
            FirstName = firstName,
            LastName = lastName,
            Patronymic = patronymic,
            UserGroupId = groupId ?? DefaultGroup.Id
        };

        Context.UserAccounts.Add(account);
        Context.UserProfiles.Add(profile);
        await Context.SaveChangesAsync();
        return profile;
    }

    public async Task<Test> CreateTestAsync(string title)
    {
        var test = new Test { Id = Guid.NewGuid(), Title = title };
        Context.Tests.Add(test);
        await Context.SaveChangesAsync();
        return test;
    }

    private async Task<Question> AddQuestionCoreAsync(Test test, QuestionType type)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = $"Вопрос ({type})",
            Type = type
        };

        var order = Context.TestQuestions.Count(tq => tq.TestId == test.Id);

        Context.Questions.Add(question);
        Context.TestQuestions.Add(new TestQuestion
        {
            TestId = test.Id,
            QuestionId = question.Id,
            Order = order
        });

        await Context.SaveChangesAsync();
        return question;
    }

    private async Task<List<AnswerOption>> AddOptionsAsync(Question question, int count)
    {
        var options = Enumerable.Range(0, count)
            .Select(i => new AnswerOption
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                Text = $"Вариант {i}",
                Order = i
            })
            .ToList();

        Context.AnswerOptions.AddRange(options);
        await Context.SaveChangesAsync();
        return options;
    }

    /// <summary>SingleChoice: один правильный вариант (индекс correctIndex).</summary>
    public async Task<(Question Question, List<AnswerOption> Options)> AddSingleChoiceAsync(
        Test test, int correctIndex, int optionCount = 3)
    {
        var question = await AddQuestionCoreAsync(test, QuestionType.SingleChoice);
        var options = await AddOptionsAsync(question, optionCount);

        Context.CorrectAnswers.Add(new CorrectAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            AnswerOptionId = options[correctIndex].Id
        });

        await Context.SaveChangesAsync();
        return (question, options);
    }

    /// <summary>TrueFalse: варианты "Верно"(0)/"Неверно"(1).</summary>
    public async Task<(Question Question, List<AnswerOption> Options)> AddTrueFalseAsync(
        Test test, bool correctIsTrue)
    {
        var question = await AddQuestionCoreAsync(test, QuestionType.TrueFalse);
        var options = await AddOptionsAsync(question, 2);

        Context.CorrectAnswers.Add(new CorrectAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            AnswerOptionId = options[correctIsTrue ? 0 : 1].Id
        });

        await Context.SaveChangesAsync();
        return (question, options);
    }

    /// <summary>MultipleChoice: правильное множество — индексы correctIndexes.</summary>
    public async Task<(Question Question, List<AnswerOption> Options)> AddMultipleChoiceAsync(
        Test test, int[] correctIndexes, int optionCount = 4)
    {
        var question = await AddQuestionCoreAsync(test, QuestionType.MultipleChoice);
        var options = await AddOptionsAsync(question, optionCount);

        foreach (var index in correctIndexes)
        {
            Context.CorrectAnswers.Add(new CorrectAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                AnswerOptionId = options[index].Id
            });
        }

        await Context.SaveChangesAsync();
        return (question, options);
    }

    /// <summary>Ordering: правильный порядок задаётся AnswerOption.Order.</summary>
    public async Task<(Question Question, List<AnswerOption> Options)> AddOrderingAsync(
        Test test, int optionCount = 3)
    {
        var question = await AddQuestionCoreAsync(test, QuestionType.Ordering);
        var options = await AddOptionsAsync(question, optionCount);
        return (question, options);
    }

    /// <summary>Matching: пары; правильный ответ — LeftPairId == RightPairId == Id пары.</summary>
    public async Task<(Question Question, List<MatchingPair> Pairs)> AddMatchingAsync(
        Test test, int pairCount = 3)
    {
        var question = await AddQuestionCoreAsync(test, QuestionType.Matching);

        var pairs = Enumerable.Range(0, pairCount)
            .Select(i => new MatchingPair
            {
                Id = Guid.NewGuid(),
                QuestionId = question.Id,
                LeftText = $"Лево {i}",
                RightText = $"Право {i}"
            })
            .ToList();

        Context.MatchingPairs.AddRange(pairs);
        await Context.SaveChangesAsync();
        return (question, pairs);
    }

    /// <summary>OpenAnswer: правильный текст (сравнение без учёта регистра/пробелов).</summary>
    public async Task<Question> AddOpenAnswerAsync(Test test, string correctText)
    {
        var question = await AddQuestionCoreAsync(test, QuestionType.OpenAnswer);

        Context.CorrectAnswers.Add(new CorrectAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Text = correctText
        });

        await Context.SaveChangesAsync();
        return question;
    }
}