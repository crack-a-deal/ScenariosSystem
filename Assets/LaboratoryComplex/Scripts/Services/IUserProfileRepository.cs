using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserProfileModel;

public interface IUserProfileRepository
{
    Task<List<UserProfile>> GetAllAsync();

    Task<UserProfile?> GetByIdAsync(Guid id);

    Task AddAsync(UserProfile profile);

    Task UpdateAsync(UserProfile profile);

    Task DeleteAsync(UserProfile profile);
}