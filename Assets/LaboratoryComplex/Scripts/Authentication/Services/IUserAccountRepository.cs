using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccountModel;

public interface IUserAccountRepository
{
    Task<IEnumerable<UserAccount>> GetAllAsync();

    Task<UserAccount?> GetByIdAsync(Guid id);

    Task<UserAccount?> GetByLoginAsync(string login);

    Task AddAsync(UserAccount account);

    Task Update(UserAccount account);

    Task<bool> DeleteAsync(Guid id);
}