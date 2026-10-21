using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UserProfileModel;

public class GroupFormPresenter
{
    public event Action GroupCreated;
    public event Action GroupUpdated;
    public event Action Closed;

    private readonly GroupFormView _view;
    private readonly IUserGroupService _service;

    private FormMode _mode = FormMode.None;
    private Guid _currentGroupId;

    public GroupFormPresenter(GroupFormView view, IUserGroupService groupService)
    {
        _view = view;
        _service = groupService;

        view.SubmitClicked += OnSubmitClicked;
        view.CancelClicked += OnCancelClicked;
    }

    public void Dispose()
    {
        _view.SubmitClicked -= OnSubmitClicked;
        _view.CancelClicked -= OnCancelClicked;
    }

    public void OpenForCreate()
    {
        _mode = FormMode.Create;
        _view.Clear();
        _view.ClearList();
        _view.ListTextActivate(true);
        _view.SetListText("Список студентов пуст");
        _view.gameObject.SetActive(true);
    }

    public async void OpenForUpdate(UserGroupDto group)
    {
        _mode = FormMode.Update;
        _currentGroupId = group.Id;

        _view.SetNumberName(group.Number.ToString());
        _view.SetCourse(group.Name);

        if (group.UserCount == 0)
        {
            _view.ClearList();
            _view.ListTextActivate(true);
            _view.SetListText("Список студентов пуст");
        }
        else
        {
            _view.ClearList();
            _view.ListTextActivate(false);
            List<UserProfile> userProfiles = await _service.GetStudentsByGroupIdAsync(group.Id);
            foreach (var item in userProfiles)
            {
                var row = _view.CreateRow();
                row.SetFullName($"{item.LastName} {item.FirstName} {item.Patronymic}");
            }
        }

        _view.gameObject.SetActive(true);
    }


    private async void OnSubmitClicked()
    {
        Debug.Log($"SubmitClicked: mode = {_mode}");

        if (_mode == FormMode.Create)
        {
            await CreateGroup();
        }
        else if (_mode == FormMode.Update)
        {
            await UpdateGroup();
        }

        _view.gameObject.SetActive(false);
    }

    private async Task CreateGroup()
    {
        Debug.Log("CreateGroup вызван");

        string number = _view.GetNumberName().Trim();
        string course = _view.GetCourse().Trim();

        if (string.IsNullOrEmpty(number))
        {
            //view.ShowError("Введите имя.");
            return;
        }

        if (string.IsNullOrEmpty(course))
        {
            //view.ShowError("Введите имя.");
            return;
        }

        try
        {
            var group = new CreateUpdateUserGroupDto(course, int.Parse(number));
            await _service.CreateAsync(group);
            GroupCreated?.Invoke();

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private async Task UpdateGroup()
    {
        Debug.Log("UpdateGroup вызван");

        string number = _view.GetNumberName().Trim();
        string course = _view.GetCourse().Trim();

        if (string.IsNullOrEmpty(number))
        {
            Debug.LogWarning("Введите номер группы.");
            return;
        }

        if (string.IsNullOrEmpty(course))
        {
            Debug.LogWarning("Введите название группы.");
            return;
        }

        if (!int.TryParse(number, out int groupNumber))
        {
            Debug.LogWarning("Номер группы должен быть числом.");
            return;
        }

        try
        {
            var group = new CreateUpdateUserGroupDto(course, groupNumber);

            await _service.UpdateAsync(_currentGroupId, group);

            GroupUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OnCancelClicked()
    {
        Closed?.Invoke();
        _view.gameObject.SetActive(false);
    }
}
