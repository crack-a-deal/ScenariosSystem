using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IThreeDModelRepository
{
    Task<List<ThreeDModel>> GetAllModelsAsync();
    Task<List<ThreeDModel>> GetViewedModelsByUserProfileIdAsync(Guid userProfileId);
    Task AddModelAsync(ThreeDModel model);
    Task AddViewAsync(UserModelView view);
    Task<ThreeDModel> GetModelByNameAsync(string name);
    Task<List<UserModelViewDto>> GetAllViewsAsync();
}