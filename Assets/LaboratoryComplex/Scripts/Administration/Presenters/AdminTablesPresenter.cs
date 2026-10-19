using System;
using System.Threading.Tasks;
using UnityEngine;

public class AdminTablesPresenter : IDisposable
{
    private readonly AdminTablesView _view;

    private readonly ComputersTablePresenter _computersPresenter;
    private readonly GroupsTablePresenter _groupsPresenter;
    private readonly StudentsTablePresenter _studentsPresenter;
    private readonly TestsTablePresenter _testsPresenter;
    private readonly StudentFormPresenter _studentFormPresenter;
    private readonly GroupFormPresenter _groupFormPresenter;
    private readonly RandomTestGenerator _testGenerator;

    private AdminTable _currentTable;

    public AdminTablesPresenter(
        AdminTablesView view,
        ComputersTablePresenter computersPresenter,
        GroupsTablePresenter groupsPresenter,
        StudentsTablePresenter studentsPresenter,
        TestsTablePresenter testsPresenter,
        StudentFormPresenter studentFormPresenter,
        GroupFormPresenter groupFormPresenter,
        RandomTestGenerator testGenerator)
    {
        _view = view;

        _computersPresenter = computersPresenter;
        _groupsPresenter = groupsPresenter;
        _studentsPresenter = studentsPresenter;
        _testsPresenter = testsPresenter;
        _studentFormPresenter = studentFormPresenter;
        _groupFormPresenter = groupFormPresenter;


        _testGenerator = testGenerator;
        _view.TableSelected += OnTableSelected;
        _view.AddClicked += OnAddClicked;

        _studentFormPresenter.StudentCreated += OnStudentCreatedUpdated;
        _studentFormPresenter.StudentUpdated += OnStudentCreatedUpdated;

        _groupFormPresenter.GroupCreated += OnGroupCreatedUpdated;
        _groupFormPresenter.GroupUpdated += OnGroupCreatedUpdated;
    }



    public void Dispose()
    {
        _view.TableSelected -= OnTableSelected;
        _view.AddClicked -= OnAddClicked;
    }

    private async void OnTableSelected(AdminTable table)
    {
        _currentTable = table;

        await ShowTable(table);
    }

    private async void OnAddClicked()
    {
        switch (_currentTable)
        {
            case AdminTable.Groups:
                _groupFormPresenter.OpenForCreate();
                break;

            case AdminTable.Students:
                _studentFormPresenter.OpenForCreate();
                break;
            case AdminTable.Tests:
                await _testGenerator.CreateAsync("Новый тест");
                await _testsPresenter.Show();
                break;
            default:
                break;
        }
    }

    private async Task ShowTable(AdminTable table)
    {
        _view.SetAddButtonVisible(table != AdminTable.Computers);

        switch (table)
        {
            case AdminTable.Computers:
                _view.SetTableLabel("Управление доступом");
                _computersPresenter.Show();
                break;

            case AdminTable.Groups:
                _view.SetTableLabel("Управление группами");
                await _groupsPresenter.ShowAsync();
                break;

            case AdminTable.Students:
                _view.SetTableLabel("Управление учащимися");
                await _studentsPresenter.ShowAsync();
                break;

            case AdminTable.Tests:
                _view.SetTableLabel("Управление тестами");
                await _testsPresenter.Show();
                break;
        }
    }

    private async void OnStudentCreatedUpdated()
    {
        _view.SetTableLabel("Управление учащимися");
        await _studentsPresenter.ShowAsync();
    }

    private async void OnGroupCreatedUpdated()
    {
        _view.SetTableLabel("Управление группами");
        await _groupsPresenter.ShowAsync();
    }
}
