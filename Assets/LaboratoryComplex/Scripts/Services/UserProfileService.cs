using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserAccountModel;
using UserProfileModel;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly IUserAccountRepository _accountRepository;
    private readonly IUserGroupRepository _groupRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserProfileService(
        IUserProfileRepository profileRepository,
        IUserAccountRepository accountRepository,
        IUserGroupRepository groupRepository,
        IPasswordHasher passwordHasher)
    {
        _profileRepository = profileRepository;
        _accountRepository = accountRepository;
        _groupRepository = groupRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserProfileDto>> GetAllAsync()
    {
        var profiles = await _profileRepository.GetAllAsync();

        return profiles.Select(ToDto).ToList();
    }

    public async Task<UserProfileDto?> GetByIdAsync(Guid id)
    {
        var profile = await _profileRepository.GetByIdAsync(id);

        if (profile == null)
            return null;

        return ToDto(profile);
    }

    public async Task<UserProfileDto> GetUserProfileIdByAccountid(Guid userAccountId)
    {
        var profiles = await _profileRepository.GetAllAsync();

        var profile = profiles.FirstOrDefault(x => x.UserAccountId == userAccountId);

        if (profile == null)
            return null;

        return ToDto(profile);
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterUserDto dto)
    {
        var existingAccount =
            await _accountRepository.GetByLoginAsync(dto.Login);

        if (existingAccount != null)
            throw new InvalidOperationException(
                "Login already exists.");

        var group =
            await _groupRepository.GetByIdAsync(
                dto.UserGroupId);

        if (group == null)
            throw new KeyNotFoundException(
                "User group not found.");

        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            PasswordHash =
                _passwordHasher.Hash(dto.Password)
        };

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserAccountId = account.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            UserGroupId = dto.UserGroupId
        };

        await _accountRepository.AddAsync(account);
        await _profileRepository.AddAsync(profile);

        var createdProfile =
            await _profileRepository.GetByIdAsync(profile.Id);

        return ToDto(createdProfile!);
    }

    public async Task UpdateAsync(
        Guid id,
        CreateUpdateUserProfileDto dto)
    {
        var profile =
            await _profileRepository.GetByIdAsync(id);

        if (profile == null)
            throw new KeyNotFoundException(
                "1 User profile not found.");

        var account =
            await _accountRepository.GetByIdAsync(
                dto.UserAccountId);

        if (account == null)
            throw new KeyNotFoundException(
                "2 User account not found.");

        var group =
            await _groupRepository.GetByIdAsync(
                dto.UserGroupId);

        if (group == null)
            throw new KeyNotFoundException(
                "3 User group not found.");

        profile.UserAccountId = dto.UserAccountId;

        profile.FirstName = dto.FirstName;
        profile.LastName = dto.LastName;
        profile.Patronymic = dto.Patronymic;

        profile.UserGroupId = dto.UserGroupId;

        await _profileRepository.UpdateAsync(profile);

    }

    public async Task DeleteAsync(Guid id)
    {
        var profile =
            await _profileRepository.GetByIdAsync(id);

        if (profile == null)
            throw new KeyNotFoundException(
                "User profile not found.");

        await _profileRepository.DeleteAsync(profile);

    }

    private static UserProfileDto ToDto(
        UserProfile profile)
    {
        return new UserProfileDto(
            profile.Id,
            profile.UserAccountId,
            profile.UserAccount.Login,
            profile.FirstName,
            profile.LastName,
            profile.Patronymic,
            profile.UserGroupId,
            profile.UserGroup.Name,
            profile.UserGroup.Number);
    }
}