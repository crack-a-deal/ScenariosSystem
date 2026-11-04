using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserAnswerSaveService
{
    private readonly AppDbContext _context;

    private readonly UserAnswerService _userAnswerService;
    private readonly UserAnswerOptionService _userAnswerOptionService;
    private readonly UserMatchingPairAnswerService _matchingPairAnswerService;

    public UserAnswerSaveService(
        UserAnswerService userAnswerService,
        UserAnswerOptionService userAnswerOptionService,
        UserMatchingPairAnswerService matchingPairAnswerService,
        AppDbContext context)
    {
        _userAnswerService = userAnswerService;
        _userAnswerOptionService = userAnswerOptionService;
        _matchingPairAnswerService = matchingPairAnswerService;
        _context = context;
    }

    public async Task SaveAsync(
        UserBaseAnswerDTO dto,
        Guid sessionId)
    {
        var sessionTestId = await _context.TestSessions
            .Where(s => s.Id == sessionId)
            .Select(s => s.TestId)
            .FirstOrDefaultAsync();

        if (sessionTestId == Guid.Empty)
            throw new InvalidOperationException(
                $"Test session {sessionId} was not found.");

        bool questionInTest = await _context.TestQuestions
            .AnyAsync(tq =>
                tq.TestId == sessionTestId &&
                tq.QuestionId == dto.QuestionId);

        if (!questionInTest)
            throw new InvalidOperationException(
                $"Question {dto.QuestionId} does not belong to the test of session {sessionId}.");

        var existing = await _context.UserAnswers
            .Include(a => a.Options)
            .Include(a => a.MatchingPairs)
            .FirstOrDefaultAsync(a =>
                a.TestSessionId == sessionId &&
                a.QuestionId == dto.QuestionId);

        if (existing != null)
        {
            _context.UserAnswerOptions.RemoveRange(existing.Options);
            _context.UserMatchingPairAnswers.RemoveRange(existing.MatchingPairs);
            _context.UserAnswers.Remove(existing);

            await _context.SaveChangesAsync();
        }

        var userAnswer = await _userAnswerService.CreateAsync(
            sessionId,
            dto.QuestionId,
            dto is UserOpenAnswerDTO openAnswerDto
                ? openAnswerDto.Text
                : null);

        switch (dto)
        {
            case UserOpenAnswerDTO:
                break;

            case UserSingleChoiceAnswerDTO singleChoice:
                await SaveSingleChoiceAsync(
                    userAnswer.Id,
                    singleChoice);
                break;

            case UserMultipleChoiceAnswerDTO multipleChoice:
                await SaveMultipleChoiceAsync(
                    userAnswer.Id,
                    multipleChoice);
                break;

            case UserTrueFalseAnswerDTO trueFalse:
                await SaveTrueFalseAsync(
                    userAnswer.Id,
                    trueFalse);
                break;

            case UserOrderingAnswerDTO ordering:
                await SaveOrderingAsync(
                    userAnswer.Id,
                    ordering);
                break;

            case UserMatchingAnswerDTO matching:
                await SaveMatchingAsync(
                    userAnswer.Id,
                    matching);
                break;

            default:
                throw new NotSupportedException(
                    $"Answer type {dto.GetType().Name} is not supported.");

        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveAllAsync(
    IEnumerable<UserBaseAnswerDTO> answers,
    Guid sessionId)
    {
        var list = answers?.ToList() ?? new List<UserBaseAnswerDTO>();

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        foreach (var answer in list)
        {
            await SaveAsync(answer, sessionId);
        }

        await transaction.CommitAsync();
    }

    private async Task SaveSingleChoiceAsync(
        Guid userAnswerId,
        UserSingleChoiceAnswerDTO dto)
    {
        if (!dto.SelectedOptionId.HasValue)
            return;

        await _userAnswerOptionService.CreateAsync(
            userAnswerId,
            dto.SelectedOptionId.Value);
    }

    private async Task SaveMultipleChoiceAsync(
        Guid userAnswerId,
        UserMultipleChoiceAnswerDTO dto)
    {
        foreach (var optionId in dto.SelectedOptionIds)
        {
            await _userAnswerOptionService.CreateAsync(
                userAnswerId,
                optionId);
        }
    }

    private async Task SaveTrueFalseAsync(
        Guid userAnswerId,
        UserTrueFalseAnswerDTO dto)
    {
        if (!dto.SelectedOptionId.HasValue)
            return;

        await _userAnswerOptionService.CreateAsync(
            userAnswerId,
            dto.SelectedOptionId.Value);
    }

    private async Task SaveOrderingAsync(
        Guid userAnswerId,
        UserOrderingAnswerDTO dto)
    {
        foreach (var item in dto.Items)
        {
            await _userAnswerOptionService.CreateAsync(
                userAnswerId,
                item.OptionId,
                item.Order);
        }
    }

    private async Task SaveMatchingAsync(
        Guid userAnswerId,
        UserMatchingAnswerDTO dto)
    {
        foreach (var pair in dto.Pairs)
        {
            await _matchingPairAnswerService.CreateAsync(
                userAnswerId,
                pair.LeftPairId,
                pair.RightPairId);
        }
    }
}