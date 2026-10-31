using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TestSessionService
{
    private readonly AppDbContext _context;
    private readonly TestSessionRepository _repository;

    public TestSessionService(AppDbContext context, TestSessionRepository repository)
    {
        _context = context;
        _repository = repository;
    }

    public async Task<TestSession> StartAsync(Guid testId, Guid userProfileId)
    {
        var session = await _context.TestSessions
            .Where(s =>
                s.TestId == testId &&
                s.UserProfileId == userProfileId &&
                s.CompletedAt == null)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        if (session == null)
        {
            throw new InvalidOperationException(
                "Для пользователя не назначена сессия этого теста.");
        }

        if (session.StartedAt == null)
        {
            session.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return session;
    }

    public async Task CompleteAsync(Guid sessionId)
    {
        await _repository.CompleteAsync(sessionId);
    }

    public async Task<TestSession?> GetByIdAsync(Guid sessionId)
    {
        return await _repository.GetByIdAsync(sessionId);
    }

    public async Task<TestSession> AssignToStudentAsync(
        Guid testId,
        Guid studentProfileId)
    {
        return await _repository.AssignToStudentAsync(
            testId,
            studentProfileId);
    }

    public async Task<List<TestSession>> AssignToAllStudentsAsync(Guid testId)
    {
        return await _repository.AssignToAllStudentsAsync(testId);
    }

    public async Task<List<TestSession>> GetByUserProfileIdAsync(Guid userProfileId)
    {
        return await _repository.GetByUserProfileIdAsync(userProfileId);
    }

    public async Task<List<TestSession>> GetByTestIdAsync(Guid testId)
    {
        return await _repository.GetByTestIdAsync(testId);
    }

    public async Task<TestSession?> GetByTestAndUserAsync(Guid testId, Guid userProfileId)
    {
        return await _repository.GetByTestAndUserAsync(testId, userProfileId);
    }
}