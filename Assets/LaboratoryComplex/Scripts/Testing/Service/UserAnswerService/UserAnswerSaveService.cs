using System;
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