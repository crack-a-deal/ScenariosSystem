
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnityEngine;

public class JsonTestImportService
{
    private readonly AppDbContext _context;
    private readonly JsonTestParser _parser;
    private readonly JsonTestMapper _mapper;

    public JsonTestImportService(AppDbContext context, JsonTestParser parser, JsonTestMapper mapper)
    {
        Debug.Log($"create JsonTestImportService");
        _context = context;
        _parser = parser;
        _mapper = mapper;
    }

    public async Task<bool> ImportAsync(string filePath)
    {
        JsonTestDTO dto = _parser.Parse(filePath);

        if (dto == null)
            return false;

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            Debug.LogError("JSON тест не содержит название.");
            return false;
        }

        if (dto.Questions == null || dto.Questions.Count == 0)
        {
            Debug.LogError("JSON тест не содержит вопросов.");
            return false;
        }

        for (int i = 0; i < dto.Questions.Count; i++)
        {
            JsonQuestionDTO question = dto.Questions[i];

            if (string.IsNullOrWhiteSpace(question.Text))
            {
                Debug.LogError(
                    $"JSON содержит вопрос без текста. Номер вопроса: {i + 1}");
                return false;
            }

            if (!IsSupportedQuestionType(question.Type))
            {
                Debug.LogError(
                    $"JSON содержит неизвестный тип вопроса: " +
                    $"'{question.Type}'. Номер вопроса: {i + 1}");
                return false;
            }
        }

        var importedQuestions = _mapper.ToEntities(dto);

        if (importedQuestions.Count != dto.Questions.Count)
        {
            Debug.LogError(
                "Не удалось преобразовать все вопросы из JSON.");
            return false;
        }

        // Не импортируем повторно вопросы с тем же текстом.
        var importedTexts = importedQuestions
            .Select(q => q.Text.Trim())
            .ToList();

        var existingTexts = await _context.Questions
            .AsNoTracking()
            .Where(q => importedTexts.Contains(q.Text.Trim()))
            .Select(q => q.Text)
            .ToListAsync();

        if (existingTexts.Count > 0)
        {
            //Debug.LogError(
            //    "Некоторые вопросы уже есть в банке. " +
            //    "Импорт отменён, чтобы не создавать дубликаты.");
            return false;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Questions.AddRangeAsync(importedQuestions);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            Debug.Log(
                $"Из JSON '{dto.Title}' импортировано " +
                $"{importedQuestions.Count} вопросов в общий банк.");

            return true;
        }
        catch (System.Exception ex)
        {
            await transaction.RollbackAsync();

            Debug.LogError($"Ошибка импорта JSON: {ex.Message}");
            return false;
        }
    }

    private bool IsSupportedQuestionType(string type)
    {
        return type == "SingleChoice"
            || type == "MultipleChoice"
            || type == "TrueFalse"
            || type == "OpenAnswer"
            || type == "Ordering"
            || type == "Matching";
    }
}
