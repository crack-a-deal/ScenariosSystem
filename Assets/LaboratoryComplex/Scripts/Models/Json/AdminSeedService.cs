using GroupModel;
using System;
using System.Linq;
using UserAccountModel;
using UserProfileModel;

public class AdminSeedService : IAdminSeedService
{
    private readonly AppDbContext _context;
    private readonly AdminSeedDataLoader _loader;

    public AdminSeedService(
        AppDbContext context,
        AdminSeedDataLoader loader)
    {
        _context = context;
        _loader = loader;
    }

    public void Seed()
    {
        AdminSeedData data = _loader.Load();

        bool adminExists =
            _context.UserAccounts.Any(
                account => account.Id == data.Account.Id);

        if (adminExists)
            return;

        UserGroup group = new UserGroup
        {
            Id = data.Group.Id,
            Name = data.Group.Name,
            Number = data.Group.Number
        };

        UserAccount account = new UserAccount
        {
            Id = data.Account.Id,
            Login = data.Account.Login,
            PasswordHash = data.Account.PasswordHash
        };

        UserProfile profile = new UserProfile
        {
            Id = data.Profile.Id,

            UserAccountId = data.Account.Id,
            UserGroupId = data.Group.Id,

            FirstName = data.Profile.FirstName,
            LastName = data.Profile.LastName,
            Patronymic = data.Profile.Patronymic
        };

        _context.UserGroups.Add(group);
        _context.UserAccounts.Add(account);
        _context.UserProfiles.Add(profile);

        _context.SaveChanges();
    }
}