using GroupModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserGroupRepository
{
    Task<List<UserGroup>> GetAllAsync();

    Task<UserGroup?> GetByIdAsync(Guid id);

    Task AddAsync(UserGroup group);

    Task Update(UserGroup group);

    Task<bool> DeleteAsync(Guid id);
}