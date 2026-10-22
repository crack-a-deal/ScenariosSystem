
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StudentAssignmentPresenter
{
    private readonly StudentAssignmentPanelView _view;
    private readonly IUserProfileService _profileService;
    private readonly TestSessionService _sessionService;

    private List<UserProfileDto> _students = new();
    private TestDTO _selectedTest;

    public StudentAssignmentPresenter(
        StudentAssignmentPanelView view,
        IUserProfileService profileService,
        TestSessionService sessionService)
    {
        _view = view;
        _profileService = profileService;
        _sessionService = sessionService;

        _view.AssignClicked += OnAssignClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    public async void Show(TestDTO test)
    {
        _selectedTest = test;
        _view.Show();
        _view.SetInteractable(false);

        try
        {
            _students = await _profileService.GetAllAsync();

            _view.SetStudents(
                _students
                    .Select(p => $"{p.LastName} {p.FirstName} {p.Patronymic}".Trim())
                    .ToList());

            _view.SetInteractable(_students.Count > 0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            _students.Clear();
            _view.SetStudents(new List<string>());
            _view.SetInteractable(false);
        }
    }

    private async void OnAssignClicked()
    {
        if (_selectedTest == null)
            return;

        int index = _view.SelectedIndex;

        if (index < 0 || index >= _students.Count)
            return;

        var student = _students[index];

        _view.SetInteractable(false);

        try
        {
            await _sessionService.AssignToStudentAsync(
                _selectedTest.Id,
                student.Id);

            Debug.Log(
                $"Тест '{_selectedTest.Title}' назначен студенту " +
                $"{student.LastName} {student.FirstName}");

            _view.Hide();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            if (_view.gameObject.activeInHierarchy)
                _view.SetInteractable(_students.Count > 0);
        }
    }

    private void OnCancelClicked()
    {
        _view.Hide();
    }
}

