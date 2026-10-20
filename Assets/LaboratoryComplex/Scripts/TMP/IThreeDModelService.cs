
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IThreeDModelService
{
    Task<List<ThreeDModelDto>> GetAllModelsAsync();
    Task<List<ThreeDModelDto>> GetViewedModelsByUserProfileIdAsync(Guid userProfileId);
    Task RegisterViewAsync(Guid userProfileId, string modelName);
    Task InitializeCatalogAsync(string[] modelNames);
    Task<ThreeDModel> GetModelByNameAsync(string name);
    Task<List<UserModelViewDto>> GetAllViewsAsync();
}
