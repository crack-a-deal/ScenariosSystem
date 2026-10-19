using System;
using System.Threading.Tasks;
using UnityEngine;

public class RoleSelectionState : BaseApplicationState
{
    private readonly RoleSelectionPresenter _roleSelectionPresenter;

    public RoleSelectionState(RoleSelectionPresenter roleSelectionPresenter, IApplication application) : base(application)
    {
        _roleSelectionPresenter = roleSelectionPresenter;
    }

    public override Task EnterAsync()
    {
        _roleSelectionPresenter.StudentSelected += OnStudentSelected;
        _roleSelectionPresenter.TeacherSelected += OnTeacherSelected;

        _roleSelectionPresenter.Show();

        return Task.CompletedTask;
    }

    public override Task ExitAsync()
    {
        _roleSelectionPresenter.StudentSelected -= OnStudentSelected;
        _roleSelectionPresenter.TeacherSelected -= OnTeacherSelected;

        _roleSelectionPresenter.Hide();

        return Task.CompletedTask;
    }

    private async void OnTeacherSelected()
    {
        await _application.ChangeStateAsync<TeacherAuthorizationState>();
    }

    private async void OnStudentSelected()
    {
        await _application.ChangeStateAsync<StudentWaitingState>();
    }
}
