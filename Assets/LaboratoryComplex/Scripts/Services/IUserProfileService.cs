using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserProfileService
{
    Task<List<UserProfileDto>> GetAllAsync();

    Task<UserProfileDto?> GetByIdAsync(Guid id);

    Task<UserProfileDto> RegisterAsync(
        RegisterUserDto dto);

    Task UpdateAsync(
        Guid id,
        CreateUpdateUserProfileDto dto);

    Task DeleteAsync(Guid id);
    Task<UserProfileDto> GetUserProfileIdByAccountid(Guid userAccountId);
}