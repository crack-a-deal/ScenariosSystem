using System;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectionView : MonoBehaviour
{
    public event Action StudentSelected;
    public event Action TeacherSelected;

    [SerializeField] private Button _studentButton;
    [SerializeField] private Button _teacherButton;


    private void Awake()
    {
        _studentButton.onClick.AddListener(OnStudentClicked);
        _teacherButton.onClick.AddListener(OnTeacherClicked);
    }

    private void OnDestroy()
    {
        _studentButton.onClick.RemoveListener(OnStudentClicked);
        _teacherButton.onClick.RemoveListener(OnTeacherClicked);
    }

    private void OnStudentClicked()
    {
        StudentSelected?.Invoke();
    }

    private void OnTeacherClicked()
    {
        TeacherSelected?.Invoke();
    }
}
