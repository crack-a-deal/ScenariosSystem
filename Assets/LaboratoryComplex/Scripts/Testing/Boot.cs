using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class Boot : MonoBehaviour
{
    [SerializeField] private TestSessionView sessionView;
    [SerializeField] private QuestionViewContainer viewContainer;
    [SerializeField] private QuestionEditViewContainer editViewContainer;


    private AppDbContext _context;

    //private async void Start()
    //{
    //    Debug.Log("[Bootstrap started...]");

    //    _context = new AppDbContext("Education.db");

    //    var databaseInitializer =
    //        new DatabaseInitializer(_context);

    //    await databaseInitializer.InitializeAsync();

    //    var testService = TestServiceFactory.Create(_context);

    //    TestDTO? testFromDb = await testService.GetTestAsync();

    //    if (testFromDb == null)
    //    {
    //        string path = Path.Combine(
    //            Application.streamingAssetsPath,
    //            "Tests/bank_electroenergetics_90_dto.json");

            //JsonTestImportService importService =
    //            JsonTestImportServiceFactory.Create(_context);

    //        bool imported = await importService.ImportAsync(path);

    //        if (!imported)
    //        {
    //            Debug.LogError("JSON вопросы не были импортированы.");
    //            return;
    //        }

    //    }

    //    if (testFromDb == null)
    //    {
    //        RandomTestGenerator generator =
    //            new RandomTestGenerator(_context);

    //        Test generatedTest =
    //            await generator.CreateAsync("Случайный тест");

    //        testFromDb =
    //            await testService.GetTestByIdAsync(generatedTest.Id);
    //    }

    //    if (testFromDb == null)
    //    {
    //        Debug.LogError("Не удалось загрузить тест из базы данных.");
    //        return;
    //    }
        
    //    var testSessionService =
    //        TestSessionServiceFactory.Create(_context);

    //    var userAnswerSaveService =
    //        UserAnswerSaveServiceFactory.Create(_context);
                    
    //    if (testFromDb == null)
    //    {
    //        Debug.LogError("Test not found.");
    //        return;
    //    }

    //    var testDto = testFromDb;

    //    var adminProfileId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    //    var session = await testSessionService.StartAsync(
    //        testDto.Id,
    //        adminProfileId);

    //    SingleChoiceQuestionDTO singleChoiceQuestion =
    //        testDto.Questions
    //            .OfType<SingleChoiceQuestionDTO>()
    //                .FirstOrDefault();

    //    MultipleChoiceQuestionDTO multipleChoiceQuestion =
    //        testDto.Questions
    //            .OfType<MultipleChoiceQuestionDTO>()
    //                .FirstOrDefault();

    //    TrueFalseQuestionDTO trueFalseQuestion =
    //        testDto.Questions
    //            .OfType<TrueFalseQuestionDTO>()
    //                .FirstOrDefault();
       
    //    OpenAnswerQuestionDTO openAnswerQuestion =
    //        testDto.Questions
    //            .OfType<OpenAnswerQuestionDTO>()
    //                .FirstOrDefault();

    //    OrderingQuestionDTO orderingQuestion =
    //        testDto.Questions
    //            .OfType<OrderingQuestionDTO>()
    //                .FirstOrDefault();

    //    MatchingQuestionDTO matchingQuestion =
    //        testDto.Questions
    //            .OfType<MatchingQuestionDTO>()
    //                .FirstOrDefault();

                
    //     _sessionPresenter = new TestSessionPresenter(sessionView, viewContainer, testSessionService, userAnswerSaveService);
    //     _sessionPresenter.ShowCurrentQuestion();

    //    SingleChoiceQuestionEditorPresenter singleChoiceQuestionEditorPresenter = new SingleChoiceQuestionEditorPresenter(editViewContainer.SingleChoice, singleChoiceQuestion);
    //    MultipleChoiceQuestionEditorPresenter multipleChoiceQuestionEditorPresenter = new MultipleChoiceQuestionEditorPresenter(editViewContainer.MultipleChoice, multipleChoiceQuestion);
    //    TrueFalseQuestionEditorPresenter trueFalseQuestionEditorPresenter = new TrueFalseQuestionEditorPresenter(editViewContainer.TrueFalse, trueFalseQuestion);
    //    OpenQuestionEditorPresenter openQuestionEditorPresenter = new OpenQuestionEditorPresenter(editViewContainer.Open, openAnswerQuestion);
    //    OrderingQuestionEditorPresenter orderingQuestionEditorPresenter = new OrderingQuestionEditorPresenter(editViewContainer.Ordering, orderingQuestion);
    //    MatchingQuestionEditorPresenter matchingQuestionEditorPresenter = new MatchingQuestionEditorPresenter(editViewContainer.Matching, matchingQuestion);
    //}
}
