using GroupModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserGroupRepository : IUserGroupRepository
{
    private readonly AppDbContext _context;

    public UserGroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserGroup>> GetAllAsync()
    {
        return await _context.UserGroups
            .Include(x => x.Users)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserGroup?> GetByIdAsync(Guid id)
    {
        return await _context.UserGroups
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(UserGroup group)
    {
        await _context.UserGroups.AddAsync(group);
        await _context.SaveChangesAsync();
    }

    public void Update(UserGroup group)
    {
        _context.UserGroups.Update(group);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var group = await _context.UserGroups
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return false;

        _context.UserGroups.Remove(group);

        await _context.SaveChangesAsync();

        return true;
    }
}