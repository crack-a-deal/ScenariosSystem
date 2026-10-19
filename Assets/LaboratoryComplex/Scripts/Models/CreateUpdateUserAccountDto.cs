public class CreateUpdateUserAccountDto
{
    public string Login { get; }
    public string? Password { get; }

    public CreateUpdateUserAccountDto(
        string login,
        string? password)
    {
        Login = login;
        Password = password;
    }
}
