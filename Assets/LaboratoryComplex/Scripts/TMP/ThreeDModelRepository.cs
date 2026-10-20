using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ThreeDModelRepository : IThreeDModelRepository
{
    private readonly AppDbContext _context;

    public ThreeDModelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ThreeDModel>> GetAllModelsAsync()
    {
        return await _context.ThreeDModels
            .AsNoTracking()
            .OrderBy(model => model.Name)
            .ToListAsync();
    }

    public async Task<List<ThreeDModel>> GetViewedModelsByUserProfileIdAsync(
        Guid userProfileId)
    {
        return await _context.UserModelViews
            .Where(view => view.UserProfileId == userProfileId)
            .Select(view => view.ThreeDModel)
            .Distinct()
            .OrderBy(model => model.Name)
            .ToListAsync();
    }

    public async Task AddModelAsync(ThreeDModel model)
    {
        await _context.ThreeDModels.AddAsync(model);
        await _context.SaveChangesAsync();
    }

    public async Task AddViewAsync(UserModelView view)
    {
        await _context.UserModelViews.AddAsync(view);
        await _context.SaveChangesAsync();
    }

    public async Task<ThreeDModel> GetModelByNameAsync(string name)
    {
        return await _context.ThreeDModels
            .FirstOrDefaultAsync(model => model.Name == name);
    }
    public async Task<List<UserModelViewDto>> GetAllViewsAsync()
    {
        return await _context.UserModelViews
            .AsNoTracking()
            .OrderByDescending(view => view.ViewedAt)
            .Select(view => new UserModelViewDto(
                view.Id,
                view.UserProfileId,
                view.ThreeDModelId,
                view.ThreeDModel.Name,
                view.ViewedAt))
            .ToListAsync();
    }
}