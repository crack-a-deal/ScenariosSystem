using GroupModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class UserGroupService : IUserGroupService
{
    private readonly IUserGroupRepository _repository;

    public UserGroupService(
        IUserGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserGroupDto>> GetAllAsync()
    {
        var groups = await _repository.GetAllAsync();

        return groups.Select(ToDto).ToList();
    }

    public async Task<UserGroupDto?> GetByIdAsync(Guid id)
    {
        var group = await _repository.GetByIdAsync(id);

        if (group == null)
            return null;

        return ToDto(group);
    }

    public async Task<UserGroupDto> CreateAsync(
        CreateUpdateUserGroupDto dto)
    {

        var group = new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Number = dto.Number
        };

        await _repository.AddAsync(group);

        return ToDto(group);
    }

    public async Task UpdateAsync(
        Guid id,
        CreateUpdateUserGroupDto dto)
    {
        var group =
            await _repository.GetByIdAsync(id);

        if (group == null)
            throw new KeyNotFoundException(
                "User group not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Group name is required.");

        group.Name = dto.Name;
        group.Number = dto.Number;

        _repository.Update(group);

    }

    public async Task<bool> DeleteGroupAsync(Guid id)
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
                $"Ошибка удаления группы: {ex}");

            return false;
        }
    }

    private static UserGroupDto ToDto(
        UserGroup group)
    {
        return new UserGroupDto(
            group.Id,
            group.Name,
            group.Number,
            group.Users.Count);
    }
}