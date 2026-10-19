using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UserAccountModel;

public class UserAccountService : IUserAccountService
{
    private readonly IUserAccountRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public UserAccountService(
        IUserAccountRepository repository,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<UserAccountDto>> GetAllAsync()
    {
        var accounts = await _repository.GetAllAsync();

        return accounts.Select(ToDto);
    }

    public async Task<UserAccountDto?> GetByIdAsync(Guid id)
    {
        var account = await _repository.GetByIdAsync(id);

        if (account == null)
            return null;

        return ToDto(account);
    }

    public async Task<UserAccountDto> CreateAsync(
        CreateUpdateUserAccountDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Login))
            throw new ArgumentException("Login is required.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password is required.");

        var existingAccount =
            await _repository.GetByLoginAsync(dto.Login);

        if (existingAccount != null)
            throw new InvalidOperationException(
                "Login already exists.");

        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            PasswordHash = _passwordHasher.Hash(dto.Password)
        };

        await _repository.AddAsync(account);

        return ToDto(account);
    }

    public async Task UpdateAsync(
        Guid id,
        CreateUpdateUserAccountDto dto)
    {
        var account = await _repository.GetByIdAsync(id);

        if (account == null)
            throw new KeyNotFoundException(
                "User account not found.");

        if (string.IsNullOrWhiteSpace(dto.Login))
            throw new ArgumentException("Login is required.");

        var existingAccount =
            await _repository.GetByLoginAsync(dto.Login);

        if (existingAccount != null &&
            existingAccount.Id != id)
        {
            throw new InvalidOperationException(
                "Login already exists.");
        }

        account.Login = dto.Login;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            account.PasswordHash =
                _passwordHasher.Hash(dto.Password);
        }

        _repository.Update(account);

    }

    public async Task<bool> DeleteAccountAsync(Guid id)
    {
        if (id == Guid.Empty)
            return false;

        try
        {
            return await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"Ошибка удаления пользователя: {ex}");

            return false;
        }
    }

    private static UserAccountDto ToDto(
        UserAccount account)
    {
        return new UserAccountDto(
            account.Id,
            account.Login);
    }
}