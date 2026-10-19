using System;
using System.Threading.Tasks;

public class UserAnswerService
{
    private readonly UserAnswerRepository _repository;

    public UserAnswerService(UserAnswerRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserAnswer> CreateAsync(
        Guid sessionId,
        Guid questionId,
        string? text = null)
    {
        return await _repository.CreateAsync(
            sessionId,
            questionId,
            text);
    }
}