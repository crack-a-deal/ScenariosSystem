using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccountModel;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly AppDbContext _context;

    public UserAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAccount>> GetAllAsync()
    {
        return await _context.UserAccounts
            .Include(x => x.Profile)
            .ThenInclude(x => x!.UserGroup)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        return await _context.UserAccounts
            .Include(x => x.Profile)
            .ThenInclude(x => x!.UserGroup)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UserAccount?> GetByLoginAsync(string login)
    {
        return await _context.UserAccounts
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Login == login);
    }

    public async Task AddAsync(UserAccount account)
    {
        await _context.UserAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAccount account)
    {
        _context.UserAccounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var users = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.Id == id);

        if (users == null)
            return false;

        _context.UserAccounts.Remove(users);

        await _context.SaveChangesAsync();

        return true;
    }
}