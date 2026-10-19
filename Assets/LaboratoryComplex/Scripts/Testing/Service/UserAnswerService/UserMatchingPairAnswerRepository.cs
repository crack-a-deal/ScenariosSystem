using System;
using System.Threading.Tasks;

public class UserMatchingPairAnswerRepository
{
    private readonly AppDbContext _context;

    public UserMatchingPairAnswerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserMatchingPairAnswer> CreateAsync(
        Guid userAnswerId,
        Guid leftPairId,
        Guid rightPairId)
    {
        var answer = new UserMatchingPairAnswer
        {
            UserAnswerId = userAnswerId,
            LeftPairId = leftPairId,
            RightPairId = rightPairId
        };

        _context.UserMatchingPairAnswers.Add(answer);

        return answer;
    }
}