public sealed class TestDto
{
    public int Id { get; }

    public string Name { get; }

    public int QuestionCount { get; }

    public TestDto(
        int id,
        string name,
        int questionCount)
    {
        Id = id;
        Name = name;
        QuestionCount = questionCount;
    }
}