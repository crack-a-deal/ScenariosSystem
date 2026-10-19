
using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonTestMapper
{
    public List<Question> ToEntities(JsonTestDTO dto)
    {
        List<Question> questions = new List<Question>();

        for (int i = 0; i < dto.Questions.Count; i++)
        {
            Question question = MapQuestion(dto.Questions[i]);

            if (question != null)
            {
                questions.Add(question);
            }
        }

        return questions;
    }

    private Question? MapQuestion(JsonQuestionDTO dto)
    {
        switch (dto.Type)
        {
            case "SingleChoice":
                return MapSingleChoice(dto);

            case "MultipleChoice":
                return MapMultipleChoice(dto);

            case "TrueFalse":
                return MapTrueFalse(dto);

            case "OpenAnswer":
                return MapOpenAnswer(dto);

            case "Ordering":
                return MapOrdering(dto);

            case "Matching":
                return MapMatching(dto);

            default:
                Debug.LogError(
                    $"Неизвестный тип вопроса в JSON: '{dto.Type}'");
                return null;
        }
    }

    private Question MapSingleChoice(JsonQuestionDTO dto)
    {
        Question question = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Type = QuestionType.SingleChoice
        };

        foreach (JsonOptionDTO jsonOption in dto.Options)
        {
            AnswerOption option = new AnswerOption
            {
                Id = Guid.NewGuid(),
                Question = question,
                Text = jsonOption.Text,
                Order = jsonOption.Order
            };

            question.Options.Add(option);

            if (jsonOption.Correct)
            {
                question.CorrectAnswers.Add(new CorrectAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    AnswerOption = option,
                    AnswerOptionId = option.Id
                });
            }
        }

        return question;
    }

    private Question MapMultipleChoice(JsonQuestionDTO dto)
    {
        Question question = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Type = QuestionType.MultipleChoice
        };

        foreach (JsonOptionDTO jsonOption in dto.Options)
        {
            AnswerOption option = new AnswerOption
            {
                Id = Guid.NewGuid(),
                Question = question,
                Text = jsonOption.Text,
                Order = jsonOption.Order
            };

            question.Options.Add(option);

            if (jsonOption.Correct)
            {
                question.CorrectAnswers.Add(new CorrectAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    AnswerOption = option,
                    AnswerOptionId = option.Id
                });
            }
        }

        return question;
    }

    private Question MapTrueFalse(JsonQuestionDTO dto)
    {
        Question question = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Type = QuestionType.TrueFalse
        };

        foreach (JsonOptionDTO jsonOption in dto.Options)
        {
            AnswerOption option = new AnswerOption
            {
                Id = Guid.NewGuid(),
                Question = question,
                Text = jsonOption.Text,
                Order = jsonOption.Order
            };

            question.Options.Add(option);

            if (jsonOption.Correct)
            {
                question.CorrectAnswers.Add(new CorrectAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    AnswerOption = option,
                    AnswerOptionId = option.Id
                });
            }
        }

        return question;
    }

    private Question MapOpenAnswer(JsonQuestionDTO dto)
    {
        Question question = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Type = QuestionType.OpenAnswer
        };

        question.CorrectAnswers.Add(new CorrectAnswer
        {
            Id = Guid.NewGuid(),
            Question = question,
            Text = dto.CorrectAnswer
        });

        return question;
    }

    private Question MapOrdering(JsonQuestionDTO dto)
    {
        Question question = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Type = QuestionType.Ordering
        };

        foreach (JsonOptionDTO jsonOption in dto.Options)
        {
            question.Options.Add(new AnswerOption
            {
                Id = Guid.NewGuid(),
                Question = question,
                Text = jsonOption.Text,
                Order = jsonOption.Order
            });
        }

        return question;
    }

    private Question MapMatching(JsonQuestionDTO dto)
    {
        Question question = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Type = QuestionType.Matching
        };

        foreach (JsonMatchingPairDTO jsonPair in dto.Pairs)
        {
            question.MatchingPairs.Add(new MatchingPair
            {
                Id = Guid.NewGuid(),
                Question = question,
                LeftText = jsonPair.LeftText,
                RightText = jsonPair.RightText
            });
        }

        return question;
    }
}
