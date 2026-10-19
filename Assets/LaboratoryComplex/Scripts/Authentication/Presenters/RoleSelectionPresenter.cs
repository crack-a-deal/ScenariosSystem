using System;

public class RoleSelectionPresenter : IDisposable
{
    public event Action StudentSelected;
    public event Action TeacherSelected;

    private readonly RoleSelectionView _view;
    public RoleSelectionPresenter(RoleSelectionView view)
    {
        _view = view;
        _view.StudentSelected += OnStudentSelected;
        _view.TeacherSelected += OnTeacherSelected;
    }

    public void Dispose()
    {
        _view.StudentSelected -= OnStudentSelected;
        _view.TeacherSelected -= OnTeacherSelected;
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void OnStudentSelected()
    {
        StudentSelected?.Invoke();
    }

    private void OnTeacherSelected()
    {
        TeacherSelected?.Invoke();
    }
}