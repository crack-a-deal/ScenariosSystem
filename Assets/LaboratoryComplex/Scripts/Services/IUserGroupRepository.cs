using GroupModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserProfileModel;

public interface IUserGroupRepository
{
    Task<List<UserGroup>> GetAllAsync();

    Task<UserGroup?> GetByIdAsync(Guid id);

    Task AddAsync(UserGroup group);

    Task Update(UserGroup group);

    Task<bool> DeleteAsync(Guid id);
    Task<List<UserProfile>> GetStudentsByGroupIdAsync(Guid groupId);
}