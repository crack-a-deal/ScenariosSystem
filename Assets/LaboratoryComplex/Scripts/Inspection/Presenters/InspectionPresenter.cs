using System;

public class InspectionPresenter : IDisposable
{
    public event Action BackClicked;

    private readonly InspectionView _view;
    private readonly InspectionPartRegistry _registry;
    private readonly InspectionSelectionController _selectionController;
    private readonly InspectionCameraController _cameraController;

    public InspectionPresenter(InspectionView view, InspectionPartRegistry registry, InspectionSelectionController selectionController, InspectionCameraController cameraController)
    {
        _view = view;
        _registry = registry;
        _selectionController = selectionController;
        _cameraController = cameraController;

        _view.BackClicked += OnBackClicked;
        _selectionController.PartSelected += HandleSelectionChanged;
    }

    public void Dispose()
    {
        _view.BackClicked -= OnBackClicked;

        _selectionController.PartSelected -= HandleSelectionChanged;
    }

    public void Show()
    {
        _registry.Refresh();

        _view.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    public void SelectModel(string model)
    {
        _registry.Select(model);
        _cameraController.FocusOn(_registry.SelectedModel);
        _registry.Refresh();

        InspectionModelInfo info = _registry.SelectedModel.GetComponent<InspectionModelInfo>();

        if (info == null)
        {
            _view.ClearModelInfo();
            return;
        }

        _view.SetModelInfo(info.Title, info.Description);
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }

    private void HandleSelectionChanged(InspectionPart part)
    {
        if (part != null)
            _cameraController.FocusOn(part.InspectionTarget);
        else
            _cameraController.ResetView();
    }
}
