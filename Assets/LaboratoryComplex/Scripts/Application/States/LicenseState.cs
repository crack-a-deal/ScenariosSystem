using System.Threading.Tasks;

public class LicenseState : BaseApplicationState
{
    public LicenseState(IApplication application) : base(application)
    {
    }

    public override Task EnterAsync()
    {
        //OnStudentSelected();
        //OnTeacherSelected();
        //if (true)
        //{
        //    OnStudentSelected();
        //}

        return Task.CompletedTask;
    }

    public override Task ExitAsync()
    {

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