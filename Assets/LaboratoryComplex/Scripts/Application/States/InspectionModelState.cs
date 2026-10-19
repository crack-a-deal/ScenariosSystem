using System.Threading.Tasks;

public class InspectionModelState : BaseApplicationState
{
    private readonly ModulesTablePresenter _tablePresenter;
    private readonly InspectionPresenter _inspectionPresenter;

    public InspectionModelState(IApplication application, ModulesTablePresenter tablePresenter, InspectionPresenter inspectionPresenter) : base(application)
    {
        _tablePresenter = tablePresenter;
        _inspectionPresenter = inspectionPresenter;
    }

    public override Task EnterAsync()
    {
        _tablePresenter.Show();

        _tablePresenter.ModuleSelected += OnModuleSelected;
        _tablePresenter.BackClicked += TablePresenterOnBackClicked;
        _inspectionPresenter.BackClicked += OnBackClicked;

        return Task.CompletedTask;
    }

    public override Task ExitAsync()
    {
        _tablePresenter.Hide();

        _tablePresenter.ModuleSelected -= OnModuleSelected;
        _tablePresenter.BackClicked -= TablePresenterOnBackClicked;
        _inspectionPresenter.BackClicked -= OnBackClicked;

        return Task.CompletedTask;
    }

    private void OnModuleSelected(string model)
    {
        _inspectionPresenter.SelectModel(model);
        _tablePresenter.Hide();
        _inspectionPresenter.Show();
    }

    private async void TablePresenterOnBackClicked()
    {
        await _application.ChangeStateAsync<StudentMainState>();
    }

    private void OnBackClicked()
    {
        _inspectionPresenter.Hide();
        _tablePresenter.Show();
    }
}
