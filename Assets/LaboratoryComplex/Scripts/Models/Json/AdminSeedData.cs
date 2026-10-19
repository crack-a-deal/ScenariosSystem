using System;

public class AdminSeedData
{
    public AdminGroupSeedData Group { get; set; }
    public AdminAccountSeedData Account { get; set; }
    public AdminProfileSeedData Profile { get; set; }
}

public class AdminGroupSeedData
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Number { get; set; }
}

public class AdminAccountSeedData
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
}

public class AdminProfileSeedData
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Patronymic { get; set; }
}