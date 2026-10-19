using System;
using System.Threading.Tasks;

public class UserAnswerOptionService
{
    private readonly UserAnswerOptionRepository _repository;

    public UserAnswerOptionService(
        UserAnswerOptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserAnswerOption> CreateAsync(
        Guid userAnswerId,
        Guid answerOptionId,
        int? order = null)
    {
        return await _repository.CreateAsync(
            userAnswerId,
            answerOptionId,
            order);
    }
}