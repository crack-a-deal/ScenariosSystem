using System;
using System.Threading.Tasks;

public class UserMatchingPairAnswerService
{
    private readonly UserMatchingPairAnswerRepository _repository;

    public UserMatchingPairAnswerService(
        UserMatchingPairAnswerRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserMatchingPairAnswer> CreateAsync(
        Guid userAnswerId,
        Guid leftPairId,
        Guid rightPairId)
    {
        return await _repository.CreateAsync(
            userAnswerId,
            leftPairId,
            rightPairId);
    }
}