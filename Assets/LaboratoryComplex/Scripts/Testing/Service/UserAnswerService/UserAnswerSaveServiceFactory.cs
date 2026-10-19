public static class UserAnswerSaveServiceFactory
{
    public static UserAnswerSaveService Create(
        AppDbContext context)
    {
        var userAnswerRepository =
            new UserAnswerRepository(context);

        var userAnswerService =
            new UserAnswerService(userAnswerRepository);

        var userAnswerOptionRepository =
            new UserAnswerOptionRepository(context);

        var userAnswerOptionService =
            new UserAnswerOptionService(
                userAnswerOptionRepository);

        var userMatchingPairAnswerRepository =
            new UserMatchingPairAnswerRepository(context);

        var userMatchingPairAnswerService =
            new UserMatchingPairAnswerService(
                userMatchingPairAnswerRepository);

        return new UserAnswerSaveService(
            userAnswerService,
            userAnswerOptionService,
            userMatchingPairAnswerService,
            context);
    }
}