using System;
using System.Threading.Tasks;

public class UserAnswerOptionRepository
{
    private readonly AppDbContext _context;

    public UserAnswerOptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAnswerOption> CreateAsync(
        Guid userAnswerId,
        Guid answerOptionId,
        int? order = null)
    {
        var answerOption = new UserAnswerOption
        {
            UserAnswerId = userAnswerId,
            AnswerOptionId = answerOptionId,
            Order = order
        };

        _context.UserAnswerOptions.Add(answerOption);

        return answerOption;
    }
}