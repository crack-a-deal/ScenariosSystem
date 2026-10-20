using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GroupsTablePresenter
{
    private readonly GroupsTableView _view;
    private readonly IUserGroupService _service;
    private readonly IExcelExportService _excelexportservice;
    private readonly GroupFormPresenter _formPresenter;
    private readonly Dictionary<GroupRowView, UserGroupDto> _rows = new();

    public GroupsTablePresenter(GroupsTableView view, IUserGroupService service, GroupFormPresenter formPresenter, IExcelExportService excelExportService)
    {
        _view = view;
        _service = service;
        _formPresenter = formPresenter;
        _excelexportservice = excelExportService;
    }

    public async Task ShowAsync()
    {
        var groups = await _service.GetAllAsync();

        _view.Clear();
        _rows.Clear();

        for (int i = 0; i < groups.Count; i++)
        {
            UserGroupDto group = groups[i];
            AddRow(group,i);
        }
    }

    private async void AddRow(UserGroupDto group,int index)
    {
        var row = _view.CreateRow();

        _rows[row] = group;

        row.SetIndex((index + 1).ToString("D2"));
        row.SetTitle(group.Name);
        row.SetStudentCount($"{group.UserCount} учащихся");

        row.ButtonClicked += button => HandleButton(row, button);
    }

    private async void HandleButton(GroupRowView row, GroupRowView.GroupRowButton button)
    {
        if (!_rows.TryGetValue(row, out var group))
            return;

        switch (button)
        {
            case GroupRowView.GroupRowButton.Edit:
                Edit(group);
                break;

            case GroupRowView.GroupRowButton.Delete:
                await DeleteAsync(group);
                break;
        }
    }

    private void Edit(UserGroupDto group)
    {
        Debug.Log(
            $"Edit group: {group.Id}, {group.Name}");

        _formPresenter.OpenForUpdate(group);
    }

    private async Task DeleteAsync(UserGroupDto group)
    {
        Debug.Log($"Delete group: {group.Id}, {group.Name}");

        if (group.UserCount > 0)
        {
            return;
        }

        bool deleted = await _service.DeleteGroupAsync(group.Id);

        if (!deleted)
        {
            Debug.LogWarning(
                $"Не удалось удалить группу {group.Name}. " +
                "Возможно, в группе есть учащиеся.");

            return;
        }

        await ShowAsync();
    }

    public async Task ExportGroupAsync()
    {
        Debug.Log($"_userAccountService = {_service}");
        Debug.Log($"_excelExportService = {_excelexportservice}");

        var group =
            await _service.GetAllAsync();

        Debug.Log($"users = {group}");

        await _excelexportservice.ExportAsync(group);
    }
}
