using Microsoft.EntityFrameworkCore;
using Networking.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UserProfileModel;

public class ScenarioRecord
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    public ICollection<UserScenarioCompletion> Completions { get; set; }
        = new List<UserScenarioCompletion>();
}

public class UserScenarioCompletion
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    [ForeignKey(nameof(UserProfileId))]
    public UserProfile UserProfile { get; set; }

    public Guid ScenarioRecordId { get; set; }

    [ForeignKey(nameof(ScenarioRecordId))]
    public ScenarioRecord Scenario { get; set; }

    public DateTime CompletedAt { get; set; }

    public double DurationSeconds { get; set; }

    public ScenarioMode Mode { get; set; }
}

public class ScenarioRecordDto
{
    public Guid Id { get; }
    public string Name { get; }

    public ScenarioRecordDto(
        Guid id,
        string name)
    {
        Id = id;
        Name = name;
    }
}

public class UserScenarioCompletionDto
{
    public Guid Id { get; }
    public Guid UserProfileId { get; }
    public Guid ScenarioRecordId { get; }
    public string ScenarioName { get; }
    public DateTime CompletedAt { get; }
    public ScenarioMode Mode { get; }
    public double DurationSeconds { get; }
    public string StudentName { get; }

    public UserScenarioCompletionDto(
        Guid id,
        Guid userProfileId,
        Guid scenarioRecordId,
        string scenarioName,
        DateTime completedAt,
        ScenarioMode mode,
        double durationSeconds,
        string studentName)
    {
        Id = id;
        UserProfileId = userProfileId;
        ScenarioRecordId = scenarioRecordId;
        ScenarioName = scenarioName;
        CompletedAt = completedAt;
        Mode = mode;
        DurationSeconds = durationSeconds;
        StudentName = studentName;
    }
}

public interface IScenarioRepository
{
    Task<List<ScenarioRecord>> GetAllScenariosAsync();
    Task<ScenarioRecord> GetScenarioByNameAsync(string name);
    Task AddScenarioAsync(ScenarioRecord scenario);
    Task AddCompletionAsync(UserScenarioCompletion completion);
    Task<List<ScenarioRecord>> GetCompletedScenariosByUserProfileIdAsync(Guid userProfileId);
    Task<List<UserScenarioCompletionDto>> GetAllCompletionsAsync();
}

public class ScenarioRepository : IScenarioRepository
{
    private readonly AppDbContext _context;

    public ScenarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScenarioRecord>> GetAllScenariosAsync()
    {
        return await _context.Scenarios
            .AsNoTracking()
            .OrderBy(scenario => scenario.Name)
            .ToListAsync();
    }

    public async Task<ScenarioRecord> GetScenarioByNameAsync(string name)
    {
        return await _context.Scenarios
            .FirstOrDefaultAsync(scenario => scenario.Name == name);
    }

    public async Task AddScenarioAsync(ScenarioRecord scenario)
    {
        await _context.Scenarios.AddAsync(scenario);
        await _context.SaveChangesAsync();
    }

    public async Task AddCompletionAsync(UserScenarioCompletion completion)
    {
        await _context.UserScenarioCompletions.AddAsync(completion);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ScenarioRecord>> GetCompletedScenariosByUserProfileIdAsync(
        Guid userProfileId)
    {
        return await _context.UserScenarioCompletions
            .Where(completion => completion.UserProfileId == userProfileId)
            .Select(completion => completion.Scenario)
            .Distinct()
            .OrderBy(scenario => scenario.Name)
            .ToListAsync();
    }

    public async Task<List<UserScenarioCompletionDto>> GetAllCompletionsAsync()
    {
        return await _context.UserScenarioCompletions
            .AsNoTracking()
            .OrderByDescending(completion => completion.CompletedAt)
            .Select(completion => new UserScenarioCompletionDto(
                completion.Id,
                completion.UserProfileId,
                completion.ScenarioRecordId,
                completion.Scenario.Name,
                completion.CompletedAt,
                completion.Mode,
                completion.DurationSeconds,
                string.Concat(
                    completion.UserProfile.LastName, " ",
                    completion.UserProfile.FirstName, " ",
                    completion.UserProfile.Patronymic ?? string.Empty)))
            .ToListAsync();
    }
}

public interface IScenarioService
{
    Task InitializeCatalogAsync(string[] scenarioNames);
    Task RegisterCompletionAsync(Guid userProfileId, string scenarioName, ScenarioMode mode, double durationSeconds);
    Task<List<ScenarioRecordDto>> GetAllScenariosAsync();
    Task<List<ScenarioRecordDto>> GetCompletedScenariosByUserProfileIdAsync(Guid userProfileId);
    Task<List<UserScenarioCompletionDto>> GetAllCompletionsAsync();
}

public class ScenarioService : IScenarioService
{
    private readonly IScenarioRepository _repository;

    public ScenarioService(IScenarioRepository repository)
    {
        _repository = repository;
    }

    public async Task InitializeCatalogAsync(string[] scenarioNames)
    {
        foreach (string scenarioName in scenarioNames.Distinct())
        {
            if (string.IsNullOrWhiteSpace(scenarioName))
                continue;

            var scenario = await _repository.GetScenarioByNameAsync(scenarioName);

            if (scenario != null)
                continue;

            await _repository.AddScenarioAsync(new ScenarioRecord
            {
                Id = Guid.NewGuid(),
                Name = scenarioName,
            });
        }
    }

    public async Task RegisterCompletionAsync(
        Guid userProfileId,
        string scenarioName,
        ScenarioMode mode,
        double durationSeconds)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException(
                "ID профиля не может быть пустым.",
                nameof(userProfileId));

        if (string.IsNullOrWhiteSpace(scenarioName))
            throw new ArgumentException(
                "Название сценария обязательно.",
                nameof(scenarioName));

        var scenario = await _repository.GetScenarioByNameAsync(scenarioName);

        if (scenario == null)
            throw new InvalidOperationException(
                $"Сценарий '{scenarioName}' не найден в каталоге.");

        await _repository.AddCompletionAsync(new UserScenarioCompletion
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            ScenarioRecordId = scenario.Id,
            CompletedAt = DateTime.Now,
            Mode = mode,
            DurationSeconds = durationSeconds > 0 ? durationSeconds : 0
        });
    }

    public async Task<List<ScenarioRecordDto>> GetAllScenariosAsync()
    {
        var scenarios = await _repository.GetAllScenariosAsync();
        var result = new List<ScenarioRecordDto>();

        foreach (var scenario in scenarios)
        {
            result.Add(new ScenarioRecordDto(
                scenario.Id,
                scenario.Name));
        }

        return result;
    }

    public async Task<List<ScenarioRecordDto>> GetCompletedScenariosByUserProfileIdAsync(
        Guid userProfileId)
    {
        var scenarios = await _repository
            .GetCompletedScenariosByUserProfileIdAsync(userProfileId);

        var result = new List<ScenarioRecordDto>();

        foreach (var scenario in scenarios)
        {
            result.Add(new ScenarioRecordDto(
                scenario.Id,
                scenario.Name));
        }

        return result;
    }

    public async Task<List<UserScenarioCompletionDto>> GetAllCompletionsAsync()
    {
        return await _repository.GetAllCompletionsAsync();
    }
}

public class RegisterScenarioCompletionRequest
    : IRequest<RegisterScenarioCompletionResponse>
{
    public Guid UserProfileId { get; set; }
    public string ScenarioName { get; set; } = string.Empty;
    public ScenarioMode Mode { get; set; }
    public double DurationSeconds { get; set; }
}

public class RegisterScenarioCompletionResponse : IResponse
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class RegisterScenarioCompletionHandler
    : IRequestHandler<RegisterScenarioCompletionRequest, RegisterScenarioCompletionResponse>
{
    private readonly IScenarioService _service;

    public RegisterScenarioCompletionHandler(IScenarioService service)
    {
        _service = service;
    }

    public async Task<RegisterScenarioCompletionResponse> HandleAsync(
        RegisterScenarioCompletionRequest request)
    {
        try
        {
            await _service.RegisterCompletionAsync(
                request.UserProfileId,
                request.ScenarioName,
                request.Mode,
                request.DurationSeconds);

            return new RegisterScenarioCompletionResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to register scenario completion: {ex}");

            return new RegisterScenarioCompletionResponse
            {
                Success = false,
                Error = "Не удалось сохранить прохождение сценария."
            };
        }
    }
}