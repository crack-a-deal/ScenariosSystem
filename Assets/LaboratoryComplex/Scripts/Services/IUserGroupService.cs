using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserProfileModel;

public interface IUserGroupService
{
    Task<List<UserGroupDto>> GetAllAsync();

    Task<UserGroupDto?> GetByIdAsync(Guid id);

    Task<UserGroupDto> CreateAsync(
        CreateUpdateUserGroupDto dto);

    Task UpdateAsync(
        Guid id,
        CreateUpdateUserGroupDto dto);

    Task<bool> DeleteGroupAsync(Guid id);
    Task<List<UserProfile>> GetStudentsByGroupIdAsync(Guid groupId);
}