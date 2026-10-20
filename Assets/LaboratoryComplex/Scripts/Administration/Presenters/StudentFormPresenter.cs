using System;
using System.Threading.Tasks;
using UnityEngine;

public class StudentFormPresenter
{
    public event Action StudentCreated;
    public event Action StudentUpdated;
    public event Action Closed;

    private readonly StudentFormView _view;
    private readonly IUserProfileService _profileService;
    private readonly IUserAccountService _accountService;
    private readonly IUserGroupService _groupService;


    private FormMode _mode = FormMode.None;
    private UserProfileDto _currentUser;

    public StudentFormPresenter(StudentFormView view, IUserProfileService profileService, IUserAccountService accountService, IUserGroupService groupService)
    {
        _view = view;
        _profileService = profileService;
        _accountService = accountService;
        _groupService = groupService;
        view.SubmitClicked += OnSubmitClicked;
        view.CancelClicked += OnCancelClicked;
    }

    public void Dispose()
    {
        _view.SubmitClicked -= OnSubmitClicked;
        _view.CancelClicked -= OnCancelClicked;
    }

    public async void OpenForCreate()
    {
        _mode = FormMode.Create;
        _view.Clear();

        try
        {
            await LoadGroupsAsync();
            _view.gameObject.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading groups: {ex.Message}");
        }

    }

    public async void OpenForUpdate(UserProfileDto user)
    {
        _mode = FormMode.Update;
        _currentUser = user;

        try
        {
            await LoadGroupsAsync();

            _view.SetLogin(user.Login);
            _view.SetPassword(string.Empty);
            _view.SetFirstName(user.FirstName);
            _view.SetLastName(user.LastName);
            _view.SetPatronymic(user.Patronymic);

            _view.SelectGroup(user.UserGroupId);

            _view.gameObject.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading groups: {ex.Message}");
        }

    }

    private async void OnSubmitClicked()
    {
        Debug.Log($"SubmitClicked: mode = {_mode}");

        if (_mode == FormMode.Create)
        {
            await CreateUser();
        }
        else if (_mode == FormMode.Update)
        {
            UpdateUser();
        }

        _view.gameObject.SetActive(false);
    }

    private async Task CreateUser()
    {
        Debug.Log("CreateUser вызван");

        string login = _view.GetLogin().Trim();
        string password = _view.GetPassword().Trim();
        string firstName = _view.GetFirstName().Trim();
        string lastName = _view.GetLastName().Trim();
        string patronymic = _view.GetPatronymic().Trim();


        Guid? groupId = _view.GetSelectedGroupId();

        if (groupId == null)
        {
            Debug.LogWarning("Не выбрана группа.");
            return;
        }

        if (string.IsNullOrEmpty(firstName))
        {
            //view.ShowError("Введите имя.");
            return;
        }

        if (string.IsNullOrEmpty(lastName))
        {
            //view.ShowError("Введите фамилию.");
            return;
        }

        if (string.IsNullOrEmpty(patronymic))
        {
            //view.ShowError("Введите отчество.");
            return;
        }

        try
        {
            var student = new RegisterUserDto(login,
                                              password,
                                              firstName,
                                              lastName,
                                              patronymic,
                                              groupId.Value);

            await _profileService.RegisterAsync(student);
            StudentCreated?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating user: {ex.Message}");
        }
    }

    private async void UpdateUser()
    {
        Debug.Log("UpdateUser вызван");

        string login = _view.GetLogin().Trim();
        string password = _view.GetPassword().Trim();
        string firstName = _view.GetFirstName().Trim();
        string lastName = _view.GetLastName().Trim();
        string patronymic = _view.GetPatronymic().Trim();

        Guid? groupId = _view.GetSelectedGroupId();

        if (groupId == null)
        {
            Debug.LogWarning("Не выбрана группа.");
            return;
        }

        if (string.IsNullOrEmpty(firstName))
        {
            //view.ShowError("Введите имя.");
            return;
        }

        if (string.IsNullOrEmpty(lastName))
        {
            //view.ShowError("Введите фамилию.");
            return;
        }

        if (string.IsNullOrEmpty(patronymic))
        {
            //view.ShowError("Введите отчество.");
            return;
        }

        try
        {
            var profileUpdate = new CreateUpdateUserProfileDto(_currentUser.UserAccountId, firstName, lastName, patronymic, groupId.Value);
            await _profileService.UpdateAsync(_currentUser.Id, profileUpdate);
            
            if (!string.IsNullOrEmpty(password))
            {
                var accountUpdate = new CreateUpdateUserAccountDto(login, password);
                await _accountService.UpdateAsync(_currentUser.UserAccountId, accountUpdate);
            }

            StudentUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating user: {ex}");
        }
    }

    private async Task LoadGroupsAsync()
    {
        var groups = await _groupService.GetAllAsync();
        _view.SetGroups(groups);
    }

    private void OnCancelClicked()
    {
        _view.gameObject.SetActive(false);
        Closed?.Invoke();
    }
}