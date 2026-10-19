using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserAccountService
{
    Task<IEnumerable<UserAccountDto>> GetAllAsync();

    Task<UserAccountDto?> GetByIdAsync(Guid id);

    Task<UserAccountDto> CreateAsync(
        CreateUpdateUserAccountDto dto);

    Task UpdateAsync(
        Guid id,
        CreateUpdateUserAccountDto dto);

    Task<bool> DeleteAccountAsync(Guid id);
}