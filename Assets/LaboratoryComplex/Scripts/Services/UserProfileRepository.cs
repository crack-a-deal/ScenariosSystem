using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserProfileModel;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserProfile>> GetAllAsync()
    {
        return await _context.UserProfiles
            .Include(x => x.UserAccount)
            .Include(x => x.UserGroup)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _context.UserProfiles
            .Include(x => x.UserAccount)
            .Include(x => x.UserGroup)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(UserProfile profile)
    {
        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
    }

    public void Update(UserProfile profile)
    {
        _context.UserProfiles.Update(profile);
    }

    public void Delete(UserProfile profile)
    {
        _context.UserProfiles.Remove(profile);
    }
}