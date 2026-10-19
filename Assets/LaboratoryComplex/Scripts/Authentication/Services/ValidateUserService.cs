using System;
using System.Threading.Tasks;
using UnityEngine;

public class ValidateUserService : IValidateUsersService
{
    private readonly IUserAccountRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateUserService(IUserAccountRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<(bool,Guid)> ValidateUserAsync(string? login, string? password)
    {
        try
        {
            var user = await _repository.GetByLoginAsync(login);

            if (user == null)
            {
                Debug.LogError($"Пользователь '{login}' НЕ НАЙДЕН в базе");
                return (false,Guid.Empty);
            }

            bool isValid = _passwordHasher.Verify(password, user.PasswordHash);

            if (!isValid)
            {
                return (false, Guid.Empty);
            }

            return (true,user.Id);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        return (false, Guid.Empty);
    }
}


