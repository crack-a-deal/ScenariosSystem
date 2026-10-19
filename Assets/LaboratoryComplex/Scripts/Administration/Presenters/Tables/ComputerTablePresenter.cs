using Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static ComputerRowView;

public class ComputersTablePresenter
{
    private readonly ComputersTableView _view;
    private readonly Dictionary<ComputerRowView, (ComputerDto Computer, Guid RequestId)> _rows = new();

    private readonly TcpServerProvider _serverProvider;
    private TcpServer _server;
    public ComputersTablePresenter(ComputersTableView view, TcpServerProvider serverProvider)
    {
        _view = view;
        _serverProvider = serverProvider;

    }

    public void Show()
    {
        _view.Clear();
        _rows.Clear();

        _server = _serverProvider.Instance ?? throw new InvalidOperationException("TCP server has not been created yet.");

        var requests = _server.GetPendingRequests<AccessRequest>();

        foreach (var request in requests)
        {
            ComputerDto computerDto = new ComputerDto(request.RequestId, request.IpAddress, request.HostName, true);
            AddRow(computerDto, request.RequestId);
        }
    }

    private void AddRow(ComputerDto computer, Guid requestId)
    {
        var row = _view.CreateRow();

        _rows[row] = (computer, requestId);

        row.SetIpAddress(computer.IpAddress);
        row.SetHostName(computer.HostName);

        row.ButtonClicked += button => HandleButton(row, button);
    }

    private void HandleButton(
        ComputerRowView row,
        ComputerRowButton button)
    {
        if (!_rows.TryGetValue(row, out var computer))
            return;

        switch (button)
        {
            case ComputerRowButton.Connect:
                Connect(computer.Computer, computer.RequestId);
                break;

            case ComputerRowButton.Disconnect:
                Disconnect(computer.Computer, computer.RequestId);
                break;
        }
    }

    private async void Connect(ComputerDto computer, Guid requestId)
    {
        try
        {
            AccessResponse response = new AccessResponse { Granted = true };

            await _server.RespondAsync(requestId, response, CancellationToken.None);

            Show();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private void Disconnect(ComputerDto computer, Guid requestId)
    {
        Debug.Log(
            $"Disconnect: {computer.IpAddress}");

        //_service.Disconnect(computer.IpAddress);

        Show();
    }
}
