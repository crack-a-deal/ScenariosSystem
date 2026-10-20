
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ThreeDModelService : IThreeDModelService
{
    private readonly IThreeDModelRepository _repository;

    public ThreeDModelService(IThreeDModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ThreeDModelDto>> GetAllModelsAsync()
    {
        var models = await _repository.GetAllModelsAsync();
        var result = new List<ThreeDModelDto>();

        foreach (var model in models)
        {
            result.Add(new ThreeDModelDto(
                model.Id,
                model.Name));
        }

        return result;
    }

    public async Task<List<ThreeDModelDto>> GetViewedModelsByUserProfileIdAsync(
        Guid userProfileId)
    {
        var models = await _repository
            .GetViewedModelsByUserProfileIdAsync(userProfileId);

        var result = new List<ThreeDModelDto>();

        foreach (var model in models)
        {
            result.Add(new ThreeDModelDto(
                model.Id,
                model.Name));
        }

        return result;
    }

    public async Task RegisterViewAsync(Guid userProfileId, string modelName)
    {
        if (userProfileId == Guid.Empty)
            throw new ArgumentException(
                "ID профиля не может быть пустым.",
                nameof(userProfileId));

        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException(
                "Название модели обязательно.",
                nameof(modelName));

        var model = await _repository.GetModelByNameAsync(modelName);

        if (model == null)
            throw new InvalidOperationException(
                $"Модель '{modelName}' не найдена в каталоге.");

        await _repository.AddViewAsync(new UserModelView
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            ThreeDModelId = model.Id,
            ViewedAt = DateTime.Now
        });
    }

    public async Task InitializeCatalogAsync(string[] modelNames)
    {
        foreach (string modelName in modelNames.Distinct())
        {
            if (string.IsNullOrWhiteSpace(modelName))
                continue;

            var model = await _repository.GetModelByNameAsync(modelName);

            if (model != null)
                continue;

            await _repository.AddModelAsync(new ThreeDModel
            {
                Id = Guid.NewGuid(),
                Name = modelName,
            });
        }
    }

    public async Task<ThreeDModel> GetModelByNameAsync(string name)
    {
        return await _repository.GetModelByNameAsync(name);
    }
    public async Task<List<UserModelViewDto>> GetAllViewsAsync()
    {
        return await _repository.GetAllViewsAsync();
    }
}
