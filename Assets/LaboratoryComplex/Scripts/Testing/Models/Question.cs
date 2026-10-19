using System;
using System.Collections.Generic;

public class Question
{
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public ICollection<TestQuestion> TestQuestions { get; set; }
        = new List<TestQuestion>();

    public ICollection<AnswerOption> Options { get; set; }
        = new List<AnswerOption>();

    public ICollection<CorrectAnswer> CorrectAnswers { get; set; }
        = new List<CorrectAnswer>();

    public ICollection<MatchingPair> MatchingPairs { get; set; }
        = new List<MatchingPair>();
}
