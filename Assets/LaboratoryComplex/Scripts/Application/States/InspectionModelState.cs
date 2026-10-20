using System.Threading.Tasks;

public class InspectionModelState : BaseApplicationState
{
    private readonly ModulesTablePresenter _tablePresenter;
    private readonly InspectionPresenter _inspectionPresenter;
    private readonly IThreeDModelService _dModelService;

    public InspectionModelState(IApplication application, ModulesTablePresenter tablePresenter, InspectionPresenter inspectionPresenter, IThreeDModelService dModelService) : base(application)
    {
        _tablePresenter = tablePresenter;
        _inspectionPresenter = inspectionPresenter;
        _dModelService = dModelService;
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

    private async void OnModuleSelected(string model)
    {
        await _inspectionPresenter.OpenModelAsync(model);

        //_inspectionPresenter.SelectModel(model);
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
