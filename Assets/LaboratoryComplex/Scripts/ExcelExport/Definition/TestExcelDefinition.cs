using System;
using System.Collections.Generic;
using System.Linq;

public class TestExcelDefinition
    : IMultiSheetExcelExportDefinition
{
    private readonly TestExcelMapper _mapper;

    private readonly QuestionExcelMapper _questionMapper;

    private readonly AnswerOptionExcelMapper _answerOptionMapper;

    private readonly MatchingPairExcelMapper _matchingPairMapper;

    public TestExcelDefinition(
        TestExcelMapper mapper,
        QuestionExcelMapper questionMapper,
        AnswerOptionExcelMapper answerOptionMapper,
        MatchingPairExcelMapper matchingPairMapper)
    {
        _mapper = mapper;
        _questionMapper = questionMapper;
        _answerOptionMapper = answerOptionMapper;
        _matchingPairMapper = matchingPairMapper;
    }

    public Type DtoType =>
        typeof(TestDTO);

    public string FileName =>
        "Tests.xlsx";

    public ExcelWorkbookData Build(object dto)
    {
        var test = (TestDTO)dto;

        var workbook =
            new ExcelWorkbookData();

        var testExcelDto =
            _mapper.Map(test);

        workbook.Sheets.Add(
            "Тест",
            new[]
            {
                testExcelDto
            });

        var questions =
            test.Questions
                .Select(question =>
                    _questionMapper.Map(question))
                .ToList();

        workbook.Sheets.Add(
                        "Вопросы",
                        questions);

        var answerOptions =
             test.Questions
                 .SelectMany(question =>
                     GetAnswerOptions(question)
                         .Select(option =>
                             _answerOptionMapper.Map(
                                 question,
                                 option)))
                 .ToList();

        workbook.Sheets.Add(
                    "Варианты ответов",
                    answerOptions);

        var matchingPairs =
            test.Questions
                .OfType<MatchingQuestionDTO>()
                .SelectMany(question =>
                    question.Pairs.Select(pair =>
                        _matchingPairMapper.Map(
                            question,
                            pair)))
                .ToList();

                workbook.Sheets.Add(
                    "Сопоставления",
                    matchingPairs);

        return workbook;
    }

    private IEnumerable<AnswerOptionDTO> GetAnswerOptions(
    BaseQuestionDTO question)
    {
        if (question is SingleChoiceQuestionDTO singleChoice)
            return singleChoice.Options;

        if (question is MultipleChoiceQuestionDTO multipleChoice)
            return multipleChoice.Options;

        if (question is TrueFalseQuestionDTO trueFalse)
            return trueFalse.Options;

        if (question is OrderingQuestionDTO ordering)
            return ordering.Options;

        return Enumerable.Empty<AnswerOptionDTO>();
    }
}
