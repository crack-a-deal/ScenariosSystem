using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class InspectionPresenter : IDisposable
{
    public event Action BackClicked;

    private readonly InspectionView _view;
    private readonly InspectionPartRegistry _registry;
    private readonly InspectionSelectionController _selectionController;
    private readonly InspectionCameraController _cameraController;
    private readonly TcpClientProvider _clientProvider;
    private readonly UserSession _userSession;


    private readonly CancellationTokenSource _lifetimeCts = new();


    public InspectionPresenter(InspectionView view,
                               InspectionPartRegistry registry,
                               InspectionSelectionController selectionController,
                               InspectionCameraController cameraController,
                               TcpClientProvider clientProvider,
                               UserSession userSession)
    {
        _view = view;
        _registry = registry;
        _selectionController = selectionController;
        _cameraController = cameraController;
        _clientProvider = clientProvider;
        _userSession = userSession;
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

    public async Task OpenModelAsync(string model)
    {
        if (model == null)
            return;

        await RegisterModelViewAsync(model);

        SelectModel(model);
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

    private async Task RegisterModelViewAsync(string model)
    {
        try
        {
            var response = await _clientProvider.Instance
                .SendRequestAsync<Register3DModelViewResponse>(
                    new Register3DModelViewRequest
                    {
                        UserProfileId = _userSession.ProfileId,
                        ModelName = model
                    },
                    _lifetimeCts.Token);

            if (!response.Success)
                Debug.LogError($"Не удалось сохранить просмотр модели: {response.Error}");
        }
        catch (OperationCanceledException)
        {
            // Запрос отменён.
        }
        catch (Exception exception)
        {
            Debug.LogError($"Не удалось зарегистрировать просмотр модели: {exception}");
        }
    }
}
