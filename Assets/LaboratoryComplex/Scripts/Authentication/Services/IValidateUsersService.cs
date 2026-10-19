using System;
using System.Threading.Tasks;

public interface IValidateUsersService
{
    Task<(bool,Guid)> ValidateUserAsync(string? login, string? password);
}
