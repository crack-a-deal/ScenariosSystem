using Networking.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.CoreUtils;

public class TestListItemDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int QuestionCount { get; set; }
}

public class GetAvailableTestsRequest : IRequest<GetAvailableTestsResponse>
{
    public Guid UserId { get; set; }
}

public class GetAvailableTestsResponse : IResponse
{
    public List<TestListItemDTO> Tests { get; set; } = new List<TestListItemDTO>();
}

public class GetAvailableTestsHandler : IRequestHandler<GetAvailableTestsRequest, GetAvailableTestsResponse>
{
    private readonly TestSessionService _sessionService;

    public GetAvailableTestsHandler(TestSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<GetAvailableTestsResponse> HandleAsync(GetAvailableTestsRequest request)
    {
        try
        {
            var sessions = await _sessionService.GetByUserProfileIdAsync(request.UserId);

            var tests = sessions
                .Where(session => session.Test != null && session.CompletedAt == null)
                .GroupBy(session => session.Test.Id)
                .Select(group => group
                    .OrderByDescending(session => session.StartedAt)
                    .First())
                .Select(session => new TestListItemDTO
                {
                    Id = session.Test.Id,
                    Title = session.Test.Title,
                    QuestionCount = session.Test.TestQuestions.Count
                })
                .ToList();


            return new GetAvailableTestsResponse
            {
                Tests = tests
            };
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        return new GetAvailableTestsResponse
        {
            Tests = new List<TestListItemDTO>()
        };
    }
}

public class GetTestRequest : IRequest<GetTestResponse>
{
    public Guid TestId { get; set; }
}

public class GetTestResponse : IResponse
{
    public TestDTO Test { get; set; }
}

public class GetTestHandler : IRequestHandler<GetTestRequest, GetTestResponse>
{
    private readonly ITestService _testService;

    public GetTestHandler(ITestService testService)
    {
        _testService = testService;
    }

    public async Task<GetTestResponse> HandleAsync(GetTestRequest request)
    {
        TestDTO test;
        try
        {
            test = await _testService.GetTestByIdAsync(request.TestId);
            Debug.Log($"try return test {test.Id} {test.Title}");
            return new GetTestResponse
            {
                Test = test
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }

        return new GetTestResponse
        {
            Test = null
        };
    }
}



public class StartTestSessionRequest : IRequest<StartTestSessionResponse>
{
    public Guid TestId { get; set; }

    public Guid UserId { get; set; }
}

public class StartTestSessionResponse : IResponse
{
    public bool Success { get; set; }

    public Guid SessionId { get; set; }

    public string Error { get; set; } = string.Empty;
}

public class StartTestRequestHandler :IRequestHandler<StartTestSessionRequest,StartTestSessionResponse>
{
    private readonly TestSessionService _testSessionService;

    public StartTestRequestHandler(TestSessionService testSessionService)
    {
        _testSessionService = testSessionService;
    }

    public async Task<StartTestSessionResponse> HandleAsync(StartTestSessionRequest request)
    {
        try
        {
            var session = await _testSessionService.StartAsync(request.TestId,request.UserId);

            return new StartTestSessionResponse
            {
                Success = true,
                SessionId = session.Id
            };
        }
        catch (Exception exception)
        {
            return new StartTestSessionResponse
            {
                Success = false,
                Error = exception.Message
            };
        }
    }
}

public class SubmitTestRequest : IRequest<SubmitTestResponse>
{
    public Guid SessionId { get; set; }

    public List<UserBaseAnswerDTO> Answers { get; set; } = new();
}

public class SubmitTestResponse : IResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
}

public class SubmitTestRequestHandler : IRequestHandler<SubmitTestRequest, SubmitTestResponse>
{
    private readonly UserAnswerSaveService _userAnswerSaveService;
    private readonly TestSessionService _testSessionService;
    private readonly TestResultService _testResultService;

    public SubmitTestRequestHandler(
        UserAnswerSaveService userAnswerSaveService,
        TestSessionService testSessionService,
        TestResultService testResultService)
    {
        _userAnswerSaveService = userAnswerSaveService;
        _testSessionService = testSessionService;
        _testResultService = testResultService;
    }

    public async Task<SubmitTestResponse> HandleAsync(SubmitTestRequest request)
    {
        try
        {
            var result = await _testResultService.CalculateAsync(
                request.SessionId,
                request.Answers);

            foreach (UserBaseAnswerDTO answer in request.Answers)
            {
                await _userAnswerSaveService.SaveAsync(
                    answer,
                    request.SessionId);
            }

            await _testSessionService.CompleteAsync(request.SessionId);

            return new SubmitTestResponse
            {
                Success = true,
                CorrectAnswers = result.CorrectAnswers,
                TotalQuestions = result.TotalQuestions
            };
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to submit test: {exception}");

            return new SubmitTestResponse
            {
                Success = false,
                ErrorMessage = "Не удалось сохранить результаты теста."
            };
        }
    }
}