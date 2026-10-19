using System;
using System.Threading.Tasks;

public class UserAnswerRepository
{
    private readonly AppDbContext _context;

    public UserAnswerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAnswer> CreateAsync(
         Guid sessionId,
         Guid questionId,
         string? text = null)
    {
        var answer = new UserAnswer
        {
            TestSessionId = sessionId,
            QuestionId = questionId,
            Text = text
        };

        _context.UserAnswers.Add(answer);

        return answer;
    }
}