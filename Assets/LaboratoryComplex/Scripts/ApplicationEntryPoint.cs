using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class ApplicationEntryPoint : IInitializable, IDisposable
{
    private readonly MyApplication _application;
    private readonly BootstrapState _bootstrapState;
    private readonly RoleSelectionState _roleSelectionState;
    private readonly TeacherAuthorizationState _teacherAuthorizationState;
    private readonly StudentAuthorizationState _studentAuthorizationState;
    private readonly TeacherMainState _teacherMainState;
    private readonly StudentWaitingState _studentWaitingState;
    private readonly StudentMainState _studentMainState;
    private readonly TestState _testState;
    private readonly InspectionModelState _inspectionModelState;
    private readonly ScenarioState _scenarioState;

    public ApplicationEntryPoint(
        MyApplication application,
        BootstrapState bootstrapState,
        RoleSelectionState roleSelectionState,
        TeacherAuthorizationState teacherAuthorizationState,
        StudentAuthorizationState studentAuthorizationState,
        TeacherMainState teacherMainState,
        StudentWaitingState studentWaitingState,
        StudentMainState studentMainState,
        TestState testState,
        InspectionModelState inspectionModelState,
        ScenarioState scenarioState)
    {
        _application = application;
        _bootstrapState = bootstrapState;
        _roleSelectionState = roleSelectionState;
        _teacherAuthorizationState = teacherAuthorizationState;
        _studentAuthorizationState = studentAuthorizationState;
        _teacherMainState = teacherMainState;
        _studentWaitingState = studentWaitingState;
        _studentMainState = studentMainState;
        _testState = testState;
        _inspectionModelState = inspectionModelState;
        _scenarioState = scenarioState;
    }

    public void Initialize()
    {
        _application.RegisterStates(new IApplicationState[]
        {
            _bootstrapState,
            _roleSelectionState,
            _teacherAuthorizationState,
            _studentAuthorizationState,
            _teacherMainState,
            _studentWaitingState,
            _studentMainState,
            _testState,
            _inspectionModelState,
            _scenarioState
        });

        _ = StartApplicationAsync();
    }

    public void Dispose()
    {
        _ = ExitApplicationAsync();
    }

    private async Task StartApplicationAsync()
    {
        try
        {
            await _application.StartAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private async Task ExitApplicationAsync()
    {
        try
        {
            await _application.Exit();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}