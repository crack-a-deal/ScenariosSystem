using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class TestingModuleFactory
{
    public static TestSessionService CreateSessionService(AppDbContext context)
    {
        return new TestSessionService(
            context,
            new TestSessionRepository(context));
    }

    public static UserAnswerSaveService CreateAnswerSaver(AppDbContext context)
    {
        return new UserAnswerSaveService(
            new UserAnswerService(new UserAnswerRepository(context)),
            new UserAnswerOptionService(new UserAnswerOptionRepository(context)),
            new UserMatchingPairAnswerService(new UserMatchingPairAnswerRepository(context)),
            context);
    }

    public static SubmitTestRequestHandler CreateSubmitHandler(AppDbContext context)
    {
        return new SubmitTestRequestHandler(
            CreateAnswerSaver(context),
            CreateSessionService(context),
            new TestResultService(context));
    }

    public static GetTestHandler CreateGetTestHandler(AppDbContext context)
    {
        return new GetTestHandler(
            new TestService(new TestRepository(context)));
    }
}

// =============================================================================
//  1. GetAvailableTestsHandler — список тестов студента
// =============================================================================

[TestFixture]
public class GetAvailableTestsHandlerTests
{
    private DbTestScope _scope = null!;
    private AppDbContext _serverContext = null!;
    private GetAvailableTestsHandler _handler = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _scope = await DbTestScope.CreateAsync();
        _serverContext = _scope.CreateReader();
        _handler = new GetAvailableTestsHandler(
            TestingModuleFactory.CreateSessionService(_serverContext));
    }

    [TearDown]
    public void TearDown()
    {
        _serverContext.Dispose();
        _scope.Dispose();
    }

    [Test]
    public async Task HandleAsync_ReturnsOnlyTestsAssignedToUser()
    {
        var student1 = await _scope.CreateProfileAsync("Иван", "Иванов");
        var student2 = await _scope.CreateProfileAsync("Пётр", "Петров");
        var test1 = await _scope.CreateTestAsync("Тест №1");
        var test2 = await _scope.CreateTestAsync("Тест №2");

        await _scope.Sessions.AssignToStudentAsync(test1.Id, student1.Id);
        await _scope.Sessions.AssignToStudentAsync(test2.Id, student2.Id);

        var forStudent1 = await _handler.HandleAsync(
            new GetAvailableTestsRequest { UserId = student1.Id });

        Assert.That(forStudent1.Tests.Count, Is.EqualTo(1));
        Assert.That(forStudent1.Tests[0].Id, Is.EqualTo(test1.Id));
        Assert.That(forStudent1.Tests[0].Title, Is.EqualTo("Тест №1"));
    }

    [Test]
    public async Task HandleAsync_ExcludesCompletedSessions()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var other = await _scope.CreateProfileAsync("Пётр", "Петров");
        var test = await _scope.CreateTestAsync("Тест");

        var completed = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);
        await _scope.Sessions.AssignToStudentAsync(test.Id, other.Id);

        await _scope.Sessions.CompleteAsync(completed.Id);

        var response = await _handler.HandleAsync(
            new GetAvailableTestsRequest { UserId = student.Id });

        Assert.That(response.Tests.Count, Is.EqualTo(0),
            "Завершённая сессия не должна показываться в списке");

        var forOther = await _handler.HandleAsync(
            new GetAvailableTestsRequest { UserId = other.Id });

        Assert.That(forOther.Tests.Count, Is.EqualTo(1),
            "Чужая завершённая сессия не должна скрывать тест у другого студента");
    }

    [Test]
    public async Task HandleAsync_DeduplicatesMultipleActiveSessionsOfSameTest()
    {
        // Повторное назначение теста = вторая активная сессия,
        // но в списке студент видит тест один раз.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Пересдача");

        await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);
        await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);

        var response = await _handler.HandleAsync(
            new GetAvailableTestsRequest { UserId = student.Id });

        Assert.That(response.Tests.Count, Is.EqualTo(1));
        Assert.That(response.Tests[0].Id, Is.EqualTo(test.Id));
    }

    [Test]
    public async Task HandleAsync_QuestionCount_IsAlwaysZero_DocumentsBug()
    {
        // ДОКУМЕНТИРУЕТ БАГ: TestSessionRepository.GetByUserProfileIdAsync
        // делает Include(x => x.Test) БЕЗ ThenInclude(t => t.TestQuestions),
        // поэтому session.Test.TestQuestions всегда пустая и QuestionCount == 0.
        // После добавления ThenInclude замените ожидание на 3.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Тест с вопросами");

        await _scope.AddSingleChoiceAsync(test, correctIndex: 0, optionCount: 2);
        await _scope.AddSingleChoiceAsync(test, correctIndex: 0, optionCount: 2);
        await _scope.AddSingleChoiceAsync(test, correctIndex: 0, optionCount: 2);

        await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);

        var response = await _handler.HandleAsync(
            new GetAvailableTestsRequest { UserId = student.Id });

        Assert.That(response.Tests.Count, Is.EqualTo(1));
        Assert.That(response.Tests[0].QuestionCount, Is.EqualTo(0),
            "Вопросы не подгружаются — счётчик всегда 0 (нет ThenInclude). " +
            "После фикса ожидание должно стать 3.");
    }

    [Test]
    public async Task HandleAsync_UnknownUser_ReturnsEmptyListWithoutThrowing()
    {
        // Ошибки внутри хендлера гасятся в пустой список —
        // неизвестный пользователь не должен ронять сервер.
        Assert.DoesNotThrowAsync(async () =>
        {
            var response = await _handler.HandleAsync(
                new GetAvailableTestsRequest { UserId = Guid.NewGuid() });
            Assert.That(response.Tests, Is.Empty);
        });
    }
}

// =============================================================================
//  2. GetTestHandler — выдача теста клиенту
// =============================================================================

[TestFixture]
public class GetTestHandlerTests
{
    private DbTestScope _scope = null!;
    private AppDbContext _serverContext = null!;
    private GetTestHandler _handler = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _scope = await DbTestScope.CreateAsync();
        _serverContext = _scope.CreateReader();
        _handler = TestingModuleFactory.CreateGetTestHandler(_serverContext);
    }

    [TearDown]
    public void TearDown()
    {
        _serverContext.Dispose();
        _scope.Dispose();
    }

    [Test]
    public async Task HandleAsync_ReturnsTestWithAllQuestions()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Полный тест");

        await _scope.AddSingleChoiceAsync(test, 0, 3);
        await _scope.AddOpenAnswerAsync(test, "Москва");
        await _scope.AddOrderingAsync(test, 3);

        var response = await _handler.HandleAsync(
            new GetTestRequest { TestId = test.Id });

        Assert.That(response.Test, Is.Not.Null);
        Assert.That(response.Test!.Id, Is.EqualTo(test.Id));
        Assert.That(response.Test.Questions.Count, Is.EqualTo(3));
        Assert.That(response.Test.Questions.Select(q => q.Type),
            Is.EquivalentTo(new[]
            {
                QuestionType.SingleChoice,
                QuestionType.OpenAnswer,
                QuestionType.Ordering
            }));
    }

    [Test]
    public async Task HandleAsync_OrdersQuestionsByTestQuestionOrder()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Порядок вопросов");

        var (q1, _) = await _scope.AddSingleChoiceAsync(test, 0, 2);
        var (q2, _) = await _scope.AddSingleChoiceAsync(test, 0, 2);
        var (q3, _) = await _scope.AddSingleChoiceAsync(test, 0, 2);

        // Хендлер стартует со списка, порядок задаём TestQuestion.Order.
        var links = await _scope.Context.TestQuestions
            .Where(tq => tq.TestId == test.Id)
            .ToListAsync();

        links.Single(l => l.QuestionId == q1.Id).Order = 2;
        links.Single(l => l.QuestionId == q2.Id).Order = 0;
        links.Single(l => l.QuestionId == q3.Id).Order = 1;
        await _scope.Context.SaveChangesAsync();

        var response = await _handler.HandleAsync(
            new GetTestRequest { TestId = test.Id });

        Assert.That(
            response.Test!.Questions.Select(q => q.Id),
            Is.EqualTo(new[] { q2.Id, q3.Id, q1.Id }),
            "Вопросы должны идти по TestQuestion.Order, а не по дате создания");
    }

    [Test]
    public async Task HandleAsync_UnknownTestId_ReturnsNullTestWithoutThrowing()
    {
        var response = await _handler.HandleAsync(
            new GetTestRequest { TestId = Guid.NewGuid() });

        Assert.That(response.Test, Is.Null);
    }

    [Test]
    public async Task HandleAsync_ShipsCorrectAnswersToClient_DocumentsSecurityIssue()
    {
        // ДОКУМЕНТИРУЕТ БАГ (утечка): GetTestHandler отправляет студенту DTO,
        // в которой TestMapper уже заполнил правильные ответы
        // (CorrectOptionId / CorrectOptionIds / CorrectAnswer).
        // Клиент (и сниффер сети) может сдать тест на 100% без знаний.
        // После введения клиентского DTO без ответов замените ассерты
        // на Is.Null / Is.Empty.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Утечка");

        var (singleQ, singleOpts) = await _scope.AddSingleChoiceAsync(test, 1, 3);
        var (multiQ, multiOpts) = await _scope.AddMultipleChoiceAsync(test, new[] { 0, 2 }, 4);
        var openQ = await _scope.AddOpenAnswerAsync(test, "Москва");

        var response = await _handler.HandleAsync(
            new GetTestRequest { TestId = test.Id });

        var single = response.Test!.Questions
            .OfType<SingleChoiceQuestionDTO>()
            .Single();
        Assert.That(single.CorrectOptionId, Is.EqualTo(singleOpts[1].Id),
            "Правильный вариант уходит на клиент (SingleChoice)");

        var multi = response.Test.Questions
            .OfType<MultipleChoiceQuestionDTO>()
            .Single();
        Assert.That(
            multi.CorrectOptionIds.ToHashSet(),
            Is.EqualTo(new HashSet<Guid> { multiOpts[0].Id, multiOpts[2].Id }),
            "Правильные варианты уходят на клиент (MultipleChoice)");

        var open = response.Test.Questions
            .OfType<OpenAnswerQuestionDTO>()
            .Single();
        Assert.That(open.CorrectAnswer, Is.EqualTo("Москва"),
            "Правильный текст ответа уходит на клиент (OpenAnswer)");
    }
}

// =============================================================================
//  3. StartTestRequestHandler — старт сессии
// =============================================================================

[TestFixture]
public class StartTestRequestHandlerTests
{
    private DbTestScope _scope = null!;
    private AppDbContext _serverContext = null!;
    private StartTestRequestHandler _handler = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _scope = await DbTestScope.CreateAsync();
        _serverContext = _scope.CreateReader();
        _handler = new StartTestRequestHandler(
            TestingModuleFactory.CreateSessionService(_serverContext));
    }

    [TearDown]
    public void TearDown()
    {
        _serverContext.Dispose();
        _scope.Dispose();
    }

    [Test]
    public async Task HandleAsync_AssignedSession_StartsSuccessfully()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Тест");
        var assigned = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);

        var response = await _handler.HandleAsync(
            new StartTestSessionRequest
            {
                TestId = test.Id,
                UserId = student.Id
            });

        Assert.That(response.Success, Is.True);
        Assert.That(response.SessionId, Is.EqualTo(assigned.Id));
        Assert.That(response.Error, Is.Empty);
    }

    [Test]
    public async Task HandleAsync_NoAssignedSession_ReturnsError()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Тест");

        var response = await _handler.HandleAsync(
            new StartTestSessionRequest
            {
                TestId = test.Id,
                UserId = student.Id
            });

        Assert.That(response.Success, Is.False);
        Assert.That(response.SessionId, Is.EqualTo(Guid.Empty));
        Assert.That(response.Error, Is.Not.Empty);
        Assert.That(response.Error, Does.Contain("назначена"));
    }

    [Test]
    public async Task HandleAsync_RepeatedStart_ReturnsSameStartedSession()
    {
        // Повторный старт не должен сбрасывать StartedAt —
        // продолжается та же попытка.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Тест");
        await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);

        var request = new StartTestSessionRequest
        {
            TestId = test.Id,
            UserId = student.Id
        };

        var first = await _handler.HandleAsync(request);
        var startedAt = (await _serverContext.TestSessions
            .AsNoTracking()
            .SingleAsync(s => s.Id == first.SessionId)).StartedAt;

        var second = await _handler.HandleAsync(request);

        Assert.That(second.Success, Is.True);
        Assert.That(second.SessionId, Is.EqualTo(first.SessionId));

        var restartedAt = (await _serverContext.TestSessions
            .AsNoTracking()
            .SingleAsync(s => s.Id == second.SessionId)).StartedAt;

        Assert.That(restartedAt, Is.EqualTo(startedAt),
            "Повторный старт не должен перезаписывать StartedAt");
    }
}

// =============================================================================
//  4. SubmitTestRequestHandler — приём ответов
// =============================================================================

[TestFixture]
public class SubmitTestRequestHandlerTests
{
    private DbTestScope _scope = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _scope = await DbTestScope.CreateAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
    }

    private async Task<(TestSession Session, Question Q1, List<AnswerOption> Opts1,
        Question Q2, List<AnswerOption> Opts2)> PrepareTwoQuestionTestAsync()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Тест из двух вопросов");

        var (q1, o1) = await _scope.AddSingleChoiceAsync(test, correctIndex: 0, optionCount: 2);
        var (q2, o2) = await _scope.AddSingleChoiceAsync(test, correctIndex: 0, optionCount: 2);

        var session = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);
        return (session, q1, o1, q2, o2);
    }

    [Test]
    public async Task HandleAsync_FullFlow_CalculatesSavesAndCompletes()
    {
        var (session, q1, o1, q2, o2) = await PrepareTwoQuestionTestAsync();

        var handler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

        var response = await handler.HandleAsync(new SubmitTestRequest
        {
            SessionId = session.Id,
            Answers = new List<UserBaseAnswerDTO>
            {
                new UserSingleChoiceAnswerDTO
                {
                    QuestionId = q1.Id,
                    SelectedOptionId = o1[0].Id
                },
                new UserSingleChoiceAnswerDTO
                {
                    QuestionId = q2.Id,
                    SelectedOptionId = o2[0].Id
                }
            }
        });

        Assert.That(response.Success, Is.True);
        Assert.That(response.ErrorMessage, Is.Null.Or.Empty);
        Assert.That(response.CorrectAnswers, Is.EqualTo(2));
        Assert.That(response.TotalQuestions, Is.EqualTo(2));

        using var reader = _scope.CreateReader();
        var stored = await reader.TestSessions
            .AsNoTracking()
            .SingleAsync(s => s.Id == session.Id);

        Assert.That(stored.CompletedAt, Is.Not.Null,
            "Сессия должна быть завершена после сдачи");

        var answers = await reader.UserAnswers
            .AsNoTracking()
            .Where(a => a.TestSessionId == session.Id)
            .ToListAsync();

        Assert.That(answers.Count, Is.EqualTo(2),
            "Оба ответа должны быть сохранены");
    }

    [Test]
    public async Task HandleAsync_WrongAnswers_CountedAsZero()
    {
        var (session, _, o1, q2, o2) = await PrepareTwoQuestionTestAsync();

        var handler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

        var response = await handler.HandleAsync(new SubmitTestRequest
        {
            SessionId = session.Id,
            Answers = new List<UserBaseAnswerDTO>
            {
                new UserSingleChoiceAnswerDTO
                {
                    QuestionId = q2.Id,
                    SelectedOptionId = o2[1].Id // неправильный
                }
            }
        });

        Assert.That(response.Success, Is.True);
        Assert.That(response.CorrectAnswers, Is.EqualTo(0));
        Assert.That(response.TotalQuestions, Is.EqualTo(2),
            "Без ответа вопрос всё равно в общем счёте");
        // o1 не используется — правильный индекс 0
        Assert.That(o1, Is.Not.Null);
    }

    [Test]
    public async Task HandleAsync_UnknownSession_ReturnsFailure()
    {
        var handler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

        var response = await handler.HandleAsync(new SubmitTestRequest
        {
            SessionId = Guid.NewGuid(),
            Answers = new List<UserBaseAnswerDTO>()
        });

        Assert.That(response.Success, Is.False);
        Assert.That(response.ErrorMessage, Is.Not.Empty);
    }

    //[Test]
    //public async Task HandleAsync_UnknownAnswerType_Fails_SessionNotCompleted()
    //{
    //    var (session, q1, o1, _, _) = await PrepareTwoQuestionTestAsync();

    //    var handler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

    //    var response = await handler.HandleAsync(new SubmitTestRequest
    //    {
    //        SessionId = session.Id,
    //        Answers = new List<UserBaseAnswerDTO>
    //        {
    //            new UserSingleChoiceAnswerDTO
    //            {
    //                QuestionId = q1.Id,
    //                SelectedOptionId = o1[0].Id
    //            },
    //            new UserUnknownTestAnswerDTO { QuestionId = q1.Id }
    //        }
    //    });

    //    Assert.That(response.Success, Is.False,
    //        "Неизвестный тип ответа должен приводить к отказу");

    //    using var reader = _scope.CreateReader();
    //    var stored = await reader.TestSessions
    //        .AsNoTracking()
    //        .SingleAsync(s => s.Id == session.Id);

    //    Assert.That(stored.CompletedAt, Is.Null,
    //        "Сессия не должна завершаться при сбойной отправке");
    //}

    //[Test]
    //public async Task HandleAsync_FailureMidway_LeavesPartialAnswers_DocumentsNoTransaction()
    //{
    //    // ДОКУМЕНТИРУЕТ БАГ: SaveChangesAsync вызывается на каждый ответ
    //    // отдельно, без общей транзакции. Ответы, идущие до сбойного,
    //    // остаются в БД. После добавления транзакции ожидание должно
    //    // стать 0.
    //    var (session, q1, o1, q2, o2) = await PrepareTwoQuestionTestAsync();

    //    var handler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

    //    await handler.HandleAsync(new SubmitTestRequest
    //    {
    //        SessionId = session.Id,
    //        Answers = new List<UserBaseAnswerDTO>
    //        {
    //            new UserSingleChoiceAnswerDTO
    //            {
    //                QuestionId = q1.Id,
    //                SelectedOptionId = o1[0].Id
    //            },
    //            new UserUnknownTestAnswerDTO { QuestionId = q2.Id } // сбой на втором
    //        }
    //    });

    //    using var reader = _scope.CreateReader();
    //    var savedCount = await reader.UserAnswers
    //        .AsNoTracking()
    //        .CountAsync(a => a.TestSessionId == session.Id);

    //    Assert.That(savedCount, Is.EqualTo(1),
    //        "Первый ответ уже записан, хотя отправка в целом провалилась " +
    //        "(нет транзакции). После фикса должно быть 0.");
    //}

    //[Test]
    //public async Task HandleAsync_RetryAfterPartialFailure_IsStuckForever_DocumentsCriticalBug()
    //{
    //    // ДОКУМЕНТИРУЕТ КРИТИЧЕСКИЙ БАГ МОДУЛЯ:
    //    // 1) первая отправка падает на середине, часть ответов записана;
    //    // 2) повторная отправка пытается INSERT те же (SessionId, QuestionId)
    //    //    и падает по уникальному индексу UserAnswer;
    //    // 3) сессия не завершается НИКОГДА — студент не может сдать тест.
    //    // После фикса (upsert/замена существующих ответов) повторная
    //    // отправка должна вернуть Success=true.
    //    var (session, q1, o1, q2, o2) = await PrepareTwoQuestionTestAsync();

    //    var firstHandler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

    //    var firstAttempt = await firstHandler.HandleAsync(new SubmitTestRequest
    //    {
    //        SessionId = session.Id,
    //        Answers = new List<UserBaseAnswerDTO>
    //        {
    //            new UserSingleChoiceAnswerDTO
    //            {
    //                QuestionId = q1.Id,
    //                SelectedOptionId = o1[0].Id
    //            },
    //            new UserUnknownTestAnswerDTO { QuestionId = q2.Id }
    //        }
    //    });
    //    Assert.That(firstAttempt.Success, Is.False);

    //    // Студент пробует снова — уже без сбойного типа ответа.
    //    var retryHandler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

    //    var retry = await retryHandler.HandleAsync(new SubmitTestRequest
    //    {
    //        SessionId = session.Id,
    //        Answers = new List<UserBaseAnswerDTO>
    //        {
    //            new UserSingleChoiceAnswerDTO
    //            {
    //                QuestionId = q1.Id,
    //                SelectedOptionId = o1[0].Id // тот же вопрос — дубликат
    //            },
    //            new UserSingleChoiceAnswerDTO
    //            {
    //                QuestionId = q2.Id,
    //                SelectedOptionId = o2[0].Id
    //            }
    //        }
    //    });

    //    Assert.That(retry.Success, Is.False,
    //        "Повторная отправка падает по уникальному индексу UserAnswer " +
    //        "(SessionId, QuestionId). Сессия застревает навсегда. " +
    //        "После фикса upsert ожидание должно стать Is.True.");

    //    using var reader = _scope.CreateReader();
    //    var stored = await reader.TestSessions
    //        .AsNoTracking()
    //        .SingleAsync(s => s.Id == session.Id);
    //    Assert.That(stored.CompletedAt, Is.Null,
    //        "Сессия так и не завершилась");
    //}

    [Test]
    public async Task HandleAsync_SecondSubmitAfterSuccess_Fails_DocumentsNoIdempotency()
    {
        // ДОКУМЕНТИРУЕТ БАГ: двойной клик «Отправить» или повтор запроса
        // после успешной сдачи возвращает ошибку вместо сохранённого
        // результата. Идемпотентность отсутствует.
        var (session, q1, o1, q2, o2) = await PrepareTwoQuestionTestAsync();

        var answers = new List<UserBaseAnswerDTO>
        {
            new UserSingleChoiceAnswerDTO
            {
                QuestionId = q1.Id,
                SelectedOptionId = o1[0].Id
            },
            new UserSingleChoiceAnswerDTO
            {
                QuestionId = q2.Id,
                SelectedOptionId = o2[0].Id
            }
        };

        var firstHandler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());
        var first = await firstHandler.HandleAsync(
            new SubmitTestRequest { SessionId = session.Id, Answers = answers });
        Assert.That(first.Success, Is.True);

        var secondHandler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());
        var second = await secondHandler.HandleAsync(
            new SubmitTestRequest { SessionId = session.Id, Answers = answers });

        Assert.That(second.Success, Is.False,
            "Повторная отправка завершённой сессии падает вместо возврата " +
            "сохранённого результата. После фикса: Success=true и те же баллы.");
    }

    [Test]
    public async Task HandleAsync_AcceptsAnswersToForeignQuestions_DocumentsTrustIssue()
    {
        // ДОКУМЕНТИРУЕТ БАГ: сервер не проверяет, что QuestionId принадлежит
        // тесту сессии — можно записать «ответ» на любой вопрос любого теста.
        // После добавления валидации этот тест должен ждать Success=false.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test1 = await _scope.CreateTestAsync("Тест 1");
        var test2 = await _scope.CreateTestAsync("Тест 2");

        var (q1, o1) = await _scope.AddSingleChoiceAsync(test1, 0, 2);
        var (foreignQ, foreignOpts) = await _scope.AddSingleChoiceAsync(test2, 0, 2);

        var session = await _scope.Sessions.AssignToStudentAsync(test1.Id, student.Id);

        var handler = TestingModuleFactory.CreateSubmitHandler(_scope.CreateReader());

        var response = await handler.HandleAsync(new SubmitTestRequest
        {
            SessionId = session.Id,
            Answers = new List<UserBaseAnswerDTO>
            {
                new UserSingleChoiceAnswerDTO
                {
                    QuestionId = q1.Id,
                    SelectedOptionId = o1[0].Id
                },
                new UserSingleChoiceAnswerDTO
                {
                    QuestionId = foreignQ.Id,
                    SelectedOptionId = foreignOpts[0].Id // вопрос ИЗ ДРУГОГО теста
                }
            }
        });

        Assert.That(response.Success, Is.True,
            "Ответ на чужой вопрос принимается без валидации");

        using var reader = _scope.CreateReader();
        var foreignSaved = await reader.UserAnswers
            .AsNoTracking()
            .AnyAsync(a =>
                a.TestSessionId == session.Id &&
                a.QuestionId == foreignQ.Id);

        Assert.That(foreignSaved, Is.True,
            "Чужой ответ сохранён в БД. После добавления валидации " +
            "ожидание должно стать Is.False.");
    }
}

// =============================================================================
//  5. TestSessionService — пересдачи и продолжение начатой сессии
// =============================================================================

[TestFixture]
public class TestSessionServiceRetakeTests
{
    private DbTestScope _scope = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _scope = await DbTestScope.CreateAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
    }

    [Test]
    public async Task StartAsync_PrefersAlreadyStartedSessionOverFreshOne()
    {
        // Два активных назначения: начатая сессия должна продолжаться,
        // а не сбрасывать прогресс студента на свежую.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Пересдача");

        var first = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);
        await _scope.SessionService.StartAsync(test.Id, student.Id); // first начата

        await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id); // fresh

        var continued = await _scope.SessionService.StartAsync(test.Id, student.Id);

        Assert.That(continued.Id, Is.EqualTo(first.Id),
            "StartAsync продолжает начатую сессию, а не перескакивает на свежую");
    }

    [Test]
    public async Task StartAsync_AfterCompletion_StartsNewAttempt()
    {
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Пересдача");

        var first = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);
        await _scope.SessionService.StartAsync(test.Id, student.Id);
        await _scope.SessionService.CompleteAsync(first.Id);

        await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id); // новая попытка

        var second = await _scope.SessionService.StartAsync(test.Id, student.Id);

        Assert.That(second.Id, Is.Not.EqualTo(first.Id));
        Assert.That(second.StartedAt, Is.Not.Null);

        using var reader = _scope.CreateReader();
        var stillCompleted = await reader.TestSessions
            .AsNoTracking()
            .SingleAsync(s => s.Id == first.Id);

        Assert.That(stillCompleted.CompletedAt, Is.Not.Null,
            "Старая попытка остаётся завершённой");
    }

    [Test]
    public async Task StartAsync_TwoFreshSessions_DoesNotThrow()
    {
        // Двойное назначение до первого входа: старт берёт любую
        // активную сессию и не падает.
        var student = await _scope.CreateProfileAsync("Иван", "Иванов");
        var test = await _scope.CreateTestAsync("Два назначения");

        var s1 = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);
        var s2 = await _scope.Sessions.AssignToStudentAsync(test.Id, student.Id);

        var started = await _scope.SessionService.StartAsync(test.Id, student.Id);

        Assert.That(
            started.Id == s1.Id || started.Id == s2.Id,
            Is.True);
        Assert.That(started.StartedAt, Is.Not.Null);
    }
}

// =============================================================================
//  6. TestMapper / QuestionDtoFactory / TestSessionTestDataFactory
// =============================================================================

[TestFixture]
public class TestMapperTests
{
    private DbTestScope _scope = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        _scope = await DbTestScope.CreateAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
    }

    private async Task<Test> LoadTestWithRelationsAsync(Guid testId)
    {
        return await _scope.Context.Tests
            .Include(t => t.TestQuestions).ThenInclude(tq => tq.Question).ThenInclude(q => q.Options)
            .Include(t => t.TestQuestions).ThenInclude(tq => tq.Question).ThenInclude(q => q.CorrectAnswers)
            .Include(t => t.TestQuestions).ThenInclude(tq => tq.Question).ThenInclude(q => q.MatchingPairs)
            .SingleAsync(t => t.Id == testId);
    }

    [Test]
    public async Task ToDto_MapsAllSixQuestionTypes()
    {
        var test = await _scope.CreateTestAsync("Все типы");

        await _scope.AddSingleChoiceAsync(test, 0, 3);
        await _scope.AddMultipleChoiceAsync(test, new[] { 0, 2 }, 4);
        await _scope.AddTrueFalseAsync(test, correctIsTrue: true);
        await _scope.AddOpenAnswerAsync(test, "Москва");
        await _scope.AddOrderingAsync(test, 3);
        await _scope.AddMatchingAsync(test, 3);

        var entity = await LoadTestWithRelationsAsync(test.Id);
        var dto = TestMapper.ToDto(entity);

        Assert.That(dto.Id, Is.EqualTo(test.Id));
        Assert.That(dto.Questions.Count, Is.EqualTo(6));
        Assert.That(dto.Questions.OfType<SingleChoiceQuestionDTO>().Count(), Is.EqualTo(1));
        Assert.That(dto.Questions.OfType<MultipleChoiceQuestionDTO>().Count(), Is.EqualTo(1));
        Assert.That(dto.Questions.OfType<TrueFalseQuestionDTO>().Count(), Is.EqualTo(1));
        Assert.That(dto.Questions.OfType<OpenAnswerQuestionDTO>().Count(), Is.EqualTo(1));
        Assert.That(dto.Questions.OfType<OrderingQuestionDTO>().Count(), Is.EqualTo(1));
        Assert.That(dto.Questions.OfType<MatchingQuestionDTO>().Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ToDto_OrdersQuestionsByOrder()
    {
        var test = await _scope.CreateTestAsync("Порядок в DTO");

        var (q1, _) = await _scope.AddSingleChoiceAsync(test, 0, 2);
        var (q2, _) = await _scope.AddSingleChoiceAsync(test, 0, 2);
        var (q3, _) = await _scope.AddSingleChoiceAsync(test, 0, 2);

        var links = await _scope.Context.TestQuestions
            .Where(tq => tq.TestId == test.Id)
            .ToListAsync();
        links.Single(l => l.QuestionId == q1.Id).Order = 2;
        links.Single(l => l.QuestionId == q2.Id).Order = 0;
        links.Single(l => l.QuestionId == q3.Id).Order = 1;
        await _scope.Context.SaveChangesAsync();

        var entity = await LoadTestWithRelationsAsync(test.Id);
        var dto = TestMapper.ToDto(entity);

        Assert.That(
            dto.Questions.Select(q => q.Id),
            Is.EqualTo(new[] { q2.Id, q3.Id, q1.Id }));
        Assert.That(dto.Questions.Select(q => q.Order),
            Is.EqualTo(new[] { 0, 1, 2 }));
    }

    [Test]
    public async Task ToDto_SingleChoiceOptions_SortedByOrder()
    {
        var test = await _scope.CreateTestAsync("Сортировка вариантов");

        var (question, options) = await _scope.AddSingleChoiceAsync(test, 1, 3);

        var shuffled = await _scope.Context.AnswerOptions
            .Where(o => o.QuestionId == question.Id)
            .ToListAsync();
        shuffled.Single(o => o.Id == options[0].Id).Order = 2;
        shuffled.Single(o => o.Id == options[1].Id).Order = 0;
        shuffled.Single(o => o.Id == options[2].Id).Order = 1;
        await _scope.Context.SaveChangesAsync();

        var entity = await LoadTestWithRelationsAsync(test.Id);
        var dto = TestMapper.ToDto(entity);

        var single = dto.Questions.OfType<SingleChoiceQuestionDTO>().Single();
        Assert.That(
            single.Options.Select(o => o.Order),
            Is.EqualTo(new[] { 0, 1, 2 }),
            "Варианты должны быть отсортированы по Order");
        Assert.That(single.Options[0].Id, Is.EqualTo(options[1].Id));
    }

    [Test]
    public void ToQuestionDto_UnsupportedQuestionType_Throws()
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Text = "Неизвестный тип",
            Type = (QuestionType)99
        };

        Assert.Throws<NotSupportedException>(
            () => TestMapper.ToQuestionDto(question, 0));
    }
}

[TestFixture]
public class QuestionDtoFactoryTests
{
    [Test]
    public void Create_ReturnsEmptyTemplateForEachType()
    {
        var types = new[]
        {
            QuestionType.SingleChoice,
            QuestionType.MultipleChoice,
            QuestionType.TrueFalse,
            QuestionType.OpenAnswer,
            QuestionType.Matching,
            QuestionType.Ordering
        };

        for (int i = 0; i < types.Length; i++)
        {
            var dto = QuestionDtoFactory.Create(types[i], order: i);

            Assert.That(dto.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(dto.Text, Is.Empty);
            Assert.That(dto.Type, Is.EqualTo(types[i]));
            Assert.That(dto.Order, Is.EqualTo(i));

            switch (dto)
            {
                case SingleChoiceQuestionDTO single:
                    Assert.That(single.Options, Is.Empty);
                    Assert.That(single.CorrectOptionId, Is.Null);
                    break;
                case TrueFalseQuestionDTO trueFalse:
                    Assert.That(trueFalse.CorrectOptionId, Is.Null);
                    break;
                case MultipleChoiceQuestionDTO multiple:
                    Assert.That(multiple.Options, Is.Empty);
                    Assert.That(multiple.CorrectOptionIds, Is.Empty);
                    break;
                case OpenAnswerQuestionDTO open:
                    Assert.That(open.CorrectAnswer, Is.Empty);
                    break;
                case MatchingQuestionDTO matching:
                    Assert.That(matching.Pairs, Is.Empty);
                    break;
                case OrderingQuestionDTO ordering:
                    Assert.That(ordering.Options, Is.Empty);
                    break;
                default:
                    Assert.Fail($"Не проверен тип DTO {dto.GetType().Name}");
                    break;
            }
        }
    }

    [Test]
    public void Create_TrueFalse_AddsTrueAndFalseOptions()
    {
        var dto = (TrueFalseQuestionDTO)QuestionDtoFactory.Create(
            QuestionType.TrueFalse, order: 0);

        Assert.That(dto.Options.Count, Is.EqualTo(2));
        Assert.That(dto.Options[0].Text, Is.EqualTo("Верно"));
        Assert.That(dto.Options[1].Text, Is.EqualTo("Неверно"));
        Assert.That(dto.Options.Select(o => o.Order), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void Create_UnknownType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuestionDtoFactory.Create((QuestionType)99, order: 0));
    }

    [Test]
    public void Create_AssignsUniqueIds()
    {
        var first = QuestionDtoFactory.Create(QuestionType.SingleChoice, 0);
        var second = QuestionDtoFactory.Create(QuestionType.SingleChoice, 0);

        Assert.That(first.Id, Is.Not.EqualTo(second.Id));
    }
}

[TestFixture]
public class TestSessionTestDataFactoryTests
{
    private TestDTO _test = null!;

    [SetUp]
    public void SetUp()
    {
        _test = new TestDTO { Id = Guid.NewGuid(), Title = "Фабрика ответов" };

        var singleOptions = Enumerable.Range(0, 3)
            .Select(i => new AnswerOptionDTO
            {
                Id = Guid.NewGuid(),
                Text = $"Вариант {i}",
                Order = i
            })
            .ToList();
        _test.Questions.Add(new SingleChoiceQuestionDTO
        {
            Id = Guid.NewGuid(),
            Type = QuestionType.SingleChoice,
            Options = singleOptions,
            CorrectOptionId = singleOptions[0].Id
        });

        var multiOptions = Enumerable.Range(0, 3)
            .Select(i => new AnswerOptionDTO
            {
                Id = Guid.NewGuid(),
                Text = $"Вариант {i}",
                Order = i
            })
            .ToList();
        _test.Questions.Add(new MultipleChoiceQuestionDTO
        {
            Id = Guid.NewGuid(),
            Type = QuestionType.MultipleChoice,
            Options = multiOptions,
            CorrectOptionIds = new List<Guid> { multiOptions[0].Id }
        });

        var tfOptions = new List<AnswerOptionDTO>
        {
            new AnswerOptionDTO { Id = Guid.NewGuid(), Text = "Верно", Order = 1 },
            new AnswerOptionDTO { Id = Guid.NewGuid(), Text = "Неверно", Order = 2 }
        };
        _test.Questions.Add(new TrueFalseQuestionDTO
        {
            Id = Guid.NewGuid(),
            Type = QuestionType.TrueFalse,
            Options = tfOptions,
            CorrectOptionId = tfOptions[0].Id
        });

        _test.Questions.Add(new OpenAnswerQuestionDTO
        {
            Id = Guid.NewGuid(),
            Type = QuestionType.OpenAnswer,
            CorrectAnswer = "Москва"
        });

        var orderOptions = new List<AnswerOptionDTO>
        {
            new AnswerOptionDTO { Id = Guid.NewGuid(), Text = "Б", Order = 1 },
            new AnswerOptionDTO { Id = Guid.NewGuid(), Text = "В", Order = 0 },
            new AnswerOptionDTO { Id = Guid.NewGuid(), Text = "А", Order = 2 }
        };
        _test.Questions.Add(new OrderingQuestionDTO
        {
            Id = Guid.NewGuid(),
            Type = QuestionType.Ordering,
            Options = orderOptions
        });

        var pairs = Enumerable.Range(0, 3)
            .Select(i => new MatchingPairDTO
            {
                Id = Guid.NewGuid(),
                LeftText = $"Лево {i}",
                RightText = $"Право {i}"
            })
            .ToList();
        _test.Questions.Add(new MatchingQuestionDTO
        {
            Id = Guid.NewGuid(),
            Type = QuestionType.Matching,
            Pairs = pairs
        });
    }

    [Test]
    public void Create_AnswersAllQuestions_ExceptSkippedIndex()
    {
        var session = TestSessionTestDataFactory.Create(_test, unansweredQuestionIndex: 2);

        Assert.That(session.Answers.Count, Is.EqualTo(5),
            "6 вопросов, один пропущен — должно быть 5 ответов");

        var all = TestSessionTestDataFactory.Create(_test);
        Assert.That(all.Answers.Count, Is.EqualTo(6));

        var byQuestion = all.Answers.ToDictionary(a => a.QuestionId);
        Assert.That(byQuestion.Keys,
            Is.EquivalentTo(_test.Questions.Select(q => q.Id)));
    }

    [Test]
    public void Create_SingleAndTrueFalse_AnswerCorrectly()
    {
        var session = TestSessionTestDataFactory.Create(_test);

        var single = session.Answers
            .OfType<UserSingleChoiceAnswerDTO>().Single();
        var expectedSingle = (SingleChoiceQuestionDTO)_test.Questions
            .Single(q => q.Type == QuestionType.SingleChoice);
        Assert.That(single.SelectedOptionId, Is.EqualTo(expectedSingle.CorrectOptionId),
            "Фабрика должна давать ПРАВИЛЬНЫЙ ответ на SingleChoice");

        var trueFalse = session.Answers
            .OfType<UserTrueFalseAnswerDTO>().Single();
        var expectedTf = (TrueFalseQuestionDTO)_test.Questions
            .Single(q => q.Type == QuestionType.TrueFalse);
        Assert.That(trueFalse.SelectedOptionId, Is.EqualTo(expectedTf.CorrectOptionId),
            "Фабрика должна давать ПРАВИЛЬНЫЙ ответ на TrueFalse");
    }

    [Test]
    public void Create_MultipleChoice_SelectsWrongOption()
    {
        var session = TestSessionTestDataFactory.Create(_test);

        var multiple = session.Answers
            .OfType<UserMultipleChoiceAnswerDTO>().Single();
        var question = (MultipleChoiceQuestionDTO)_test.Questions
            .Single(q => q.Type == QuestionType.MultipleChoice);

        Assert.That(multiple.SelectedOptionIds.Count, Is.EqualTo(1));
        Assert.That(
            question.CorrectOptionIds.Contains(multiple.SelectedOptionIds[0]),
            Is.False,
            "Фабрика должна давать НЕПРАВИЛЬНЫЙ ответ на MultipleChoice");
    }

    [Test]
    public void Create_OpenAnswer_TextIsWrongPlaceholder()
    {
        var session = TestSessionTestDataFactory.Create(_test);

        var open = session.Answers
            .OfType<UserOpenAnswerDTO>().Single();

        Assert.That(open.Text, Is.Not.EqualTo("Москва"));
        Assert.That(open.Text, Is.Not.Empty);
    }

    [Test]
    public void Create_Ordering_FollowsOptionsOrderBy()
    {
        var session = TestSessionTestDataFactory.Create(_test);

        var ordering = session.Answers
            .OfType<UserOrderingAnswerDTO>().Single();
        var question = (OrderingQuestionDTO)_test.Questions
            .Single(q => q.Type == QuestionType.Ordering);

        var expectedIds = question.Options
            .OrderBy(o => o.Order)
            .Select(o => o.Id)
            .ToList();

        Assert.That(
            ordering.Items.OrderBy(i => i.Order).Select(i => i.OptionId),
            Is.EqualTo(expectedIds),
            "Фабрика должна давать ПРАВИЛЬНЫЙ порядок");

        Assert.That(ordering.Items.Select(i => i.Order),
            Is.EqualTo(new[] { 1, 2, 3 }),
            "Order заполняется с 1 (заметка: TestResultService сравнивает " +
            "последовательности, поэтому 1-базная нумерация безопасна)");
    }

    [Test]
    public void Create_Matching_PairsAreShifted()
    {
        var session = TestSessionTestDataFactory.Create(_test);

        var matching = session.Answers
            .OfType<UserMatchingAnswerDTO>().Single();

        Assert.That(matching.Pairs.Count, Is.EqualTo(3));
        Assert.That(matching.Pairs.All(p => p.LeftPairId != p.RightPairId),
            Is.True,
            "Фабрика должна давать НЕПРАВИЛЬНОЕ сопоставление (сдвиг пар)");
    }
}

// =============================================================================
//  7. ChoiceOptionView — UI-баг: событие выбора не срабатывает
// =============================================================================

[TestFixture]
public class ChoiceOptionViewTests
{
    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null,
            $"Поле {fieldName} не найдено — проверьте ChoiceOptionView.cs");
        field!.SetValue(target, value);
    }

    [Test]
    public void ValueChanged_NeverFires_DocumentsBug()
    {
        // ДОКУМЕНТИРУЕТ КРИТИЧЕСКИЙ UI-БАГ:
        // ChoiceOptionView.OnToggleValueChanged пустой (вызов
        // ValueChanged?.Invoke закомментирован), поэтому выбор вариантов
        // Single/Multiple/TrueFalse не доходит до презентеров, и студент
        // всегда сдаёт ПУСТЫЕ ответы по этим типам.
        // После фикса замените Is.False на Is.True.
        var go = new GameObject("choice-option-under-test");
        try
        {
            go.SetActive(false); // OnEnable не должен сработать до подстановки toggle

            var view = go.AddComponent<ChoiceOptionView>();
            var toggle = go.AddComponent<Toggle>();

            SetPrivateField(view, "toggle", toggle);

            go.SetActive(true); // теперь OnEnable подпишется на toggle

            bool fired = false;
            view.ValueChanged += (optionView, isOn) => fired = true;

            toggle.isOn = true;

            Assert.That(fired, Is.False,
                "Событие выбора варианта не срабатывает — OnToggleValueChanged " +
                "пустой. После фикса ожидание должно стать Is.True, и тест " +
                "уронит сборку, если событие снова потеряется.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}