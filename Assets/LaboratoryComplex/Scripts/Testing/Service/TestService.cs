
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TestService : ITestService
{
    private readonly TestRepository _testRepository;

    public TestService(TestRepository testRepository)
    {
        _testRepository = testRepository;
    }

    public async Task<TestDTO?> GetTestAsync()
    {
        var test = await _testRepository.GetFirstWithQuestionsAsync();

        return test == null ? null : TestMapper.ToDto(test);
    }

    public async Task<TestDTO?> GetTestByIdAsync(Guid testId)
    {
        var test = await _testRepository.GetByIdWithQuestionsAsync(testId);

        return test == null ? null : TestMapper.ToDto(test);
    }

    public async Task<List<TestDTO>> GetAllTestsAsync()
    {
        var tests = await _testRepository.GetAllAsync();

        return tests
            .Select(TestMapper.ToDto)
            .ToList();
    }

    public async Task<List<TestListItemDTO>> GetAvailableTestsAsync()
    {
        try
        {
            List<Test> tests = await _testRepository.GetAllAsync();
            return tests
            .Select(test => new TestListItemDTO
            {
                Id = test.Id,
                Title = test.Title,
                QuestionCount = test.TestQuestions.Count
            })
            .ToList();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        return null;

    }

    public async Task DeleteTestAsync(Guid testId)
    {
        await _testRepository.DeleteAsync(testId);
    }

    public async Task SaveAsync(TestDTO test)
    {
        if (test == null)
            throw new ArgumentNullException(nameof(test));

        if (string.IsNullOrWhiteSpace(test.Title))
            throw new ArgumentException("Название теста не может быть пустым.", nameof(test));

        if (test.Questions == null || test.Questions.Count == 0)
            throw new ArgumentException("Тест должен содержать хотя бы один вопрос.", nameof(test));

        await _testRepository.SaveAsync(test);
    }

    public async Task<List<BaseQuestionDTO>> GetAvailableQuestionsAsync()
    {
        var questions = await _testRepository.GetAllQuestionsAsync();

        return questions
            .Select(q => TestMapper.ToQuestionDto(q,0))
            .ToList();
    }
}
