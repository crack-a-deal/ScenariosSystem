using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TestSessionRepository
{
    private readonly AppDbContext _context;

    public TestSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TestSession> CreateAsync(Guid testId, Guid userProfileId)
    {
        Debug.Log("CREATE SESSION");
        var session = new TestSession
        {
            TestId = testId,
            UserProfileId = userProfileId,
        };

        _context.TestSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<TestSession> AssignToStudentAsync(
    Guid testId,
    Guid studentProfileId)
    {
        bool testExists = await _context.Tests
            .AnyAsync(x => x.Id == testId);

        if (!testExists)
            throw new InvalidOperationException(
                $"Test {testId} was not found.");

        bool studentExists = await _context.UserProfiles
            .AnyAsync(x => x.Id == studentProfileId);

        if (!studentExists)
            throw new InvalidOperationException(
                $"Student {studentProfileId} was not found.");

        bool alreadyAssigned = await _context.TestSessions
            .AnyAsync(x =>
                x.TestId == testId &&
                x.UserProfileId == studentProfileId);

        if (alreadyAssigned)
            throw new InvalidOperationException(
                "Этот тест уже назначен данному студенту.");

        var session = new TestSession
        {
            TestId = testId,
            UserProfileId = studentProfileId
        };

        _context.TestSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<List<TestSession>> AssignToAllStudentsAsync(Guid testId)
    {
        bool testExists = await _context.Tests
            .AnyAsync(x => x.Id == testId);

        if (!testExists)
            throw new InvalidOperationException($"Test {testId} was not found.");

        var assignedProfileIds = await _context.TestSessions
            .Where(x => x.TestId == testId)
            .Select(x => x.UserProfileId)
            .Distinct()
            .ToListAsync();

        var students = await _context.UserProfiles
            .Where(x => !assignedProfileIds.Contains(x.Id))
            .ToListAsync();

        var sessions = students.Select(student => new TestSession
        {
            TestId = testId,
            UserProfileId = student.Id,
        }).ToList();

        if (sessions.Count == 0)
            return sessions;

        _context.TestSessions.AddRange(sessions);
        await _context.SaveChangesAsync();

        return sessions;
    }

    public async Task<List<TestSession>> GetByUserProfileIdAsync(Guid userProfileId)
    {
        return await _context.TestSessions
            .Where(x => x.UserProfileId == userProfileId)
            .Include(x => x.Test)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();
    }

    public async Task CompleteAsync(Guid sessionId)
    {
        var session = await _context.TestSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
            return;

        session.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<TestSession?> GetByIdAsync(Guid sessionId)
    {
        return await _context.TestSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId);
    }

    public async Task<List<TestSession>> GetByTestIdAsync(Guid testId)
    {
        return await _context.TestSessions
            .AsNoTracking()
            .Where(x => x.TestId == testId)
            .Include(x => x.UserProfile)
            .Include(x => x.Answers)
            .OrderBy(x => x.UserProfileId)
            .ThenByDescending(x => x.StartedAt)
            .ToListAsync();
    }

    public async Task<TestSession?> GetByTestAndUserAsync(Guid testId, Guid userProfileId)
    {
        return await _context.TestSessions
            .AsNoTracking()
            .Include(x => x.Test)
            .Include(x => x.UserProfile)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x =>
                x.TestId == testId &&
                x.UserProfileId == userProfileId);
    }
}