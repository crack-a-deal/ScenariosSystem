
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static StudentRowView;

public class StudentsTablePresenter
{
    private readonly StudentsTableView _view;

    private readonly IUserAccountService _accountservice;
    private readonly IUserProfileService _profileService;
    private readonly IExcelExportService _excelexportservice;
    private readonly StudentFormPresenter _formPresenter;
    private readonly TeacherLearningSectionsPresenter _sectionsPresenter;
    private readonly Dictionary<StudentRowView, UserProfileDto> _rows = new();

    public StudentsTablePresenter(StudentsTableView view,
                                      IUserAccountService accountservice,
                                      IUserProfileService profileService,
                                      StudentFormPresenter formPresenter,
                                      TeacherLearningSectionsPresenter sectionsPresenter,
                                      IExcelExportService excelexportservice)
    {
        _view = view;
        _accountservice = accountservice;
        _profileService = profileService;
        _excelexportservice = excelexportservice;
        _formPresenter = formPresenter;
        _sectionsPresenter = sectionsPresenter;
    }

    public async Task ShowAsync()
    {
        var students = await _profileService.GetAllAsync();

        _view.Clear();
        _rows.Clear();


        for (int i = 0; i < students.Count; i++)
        {
            UserProfileDto student = students[i];
            AddRow(student, i);
        }
    }

    private async void AddRow(UserProfileDto student, int index)
    {
        var row = _view.CreateRow();

        _rows[row] = student;

        row.SetIndex((index + 1).ToString("D2"));
        row.SetFullName($"{student.LastName} {student.FirstName} {student.Patronymic}");
        row.SetGroup(student.GroupName);

        row.ButtonClicked +=
            button => HandleButton(row, button);
    }

    private async void HandleButton(
        StudentRowView row,
        StudentRowButton button)
    {
        if (!_rows.TryGetValue(row, out var student))
            return;

        switch (button)
        {
            case StudentRowButton.Result:
                ShowResult(student);
                break;

            case StudentRowButton.Edit:
                Edit(student);
                break;

            case StudentRowButton.Delete:
                await DeleteAsync(student);
                break;
        }
    }

    private void ShowResult(UserProfileDto student)
    {
        _sectionsPresenter.SetUser(student);
        _sectionsPresenter.Show();
    }

    private void Edit(UserProfileDto student)
    {
        Debug.Log(
            $"Edit student: {student.Id}, {student.Login}");

        _formPresenter.OpenForUpdate(student);
    }

    private async Task DeleteAsync(UserProfileDto student)
    {
        Debug.Log(
            $"Delete student: {student.UserAccountId}, {student.Login}");

        await _accountservice.DeleteAccountAsync(student.UserAccountId);

        await ShowAsync();
    }
}
