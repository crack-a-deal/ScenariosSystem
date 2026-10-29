using Networking;
using Networking.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static ComputerRowView;

public class ComputersTablePresenter : IDisposable
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
        Debug.Log("SHOW");

        Debug.Log($"server:{_server}");
        if (_server == null)
        {
            _server = _serverProvider.Instance ?? throw new InvalidOperationException("TCP server has not been created yet.");

            //Subscribe(_server);
        }
        

        UpdateList();
    }

    private void UpdateList()
    {
        Debug.Log("Draw");
        Debug.Log($"server:{_server}");

        _view.Clear();
        _rows.Clear();
        try
        {
        var requests = _server.GetPendingRequests<AccessRequest>();
        Debug.Log($"request {requests.Count}");

        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }
        var pendingRequests = _server
                    .GetPendingRequests<AccessRequest>()
                    .ToDictionary(request => request.ClientId);
        Debug.Log("clients: "+_server.ClientRegistry.Clients.Count);
        foreach (var client in _server.ClientRegistry.Clients)
        {
            bool hasPendingRequest = pendingRequests.TryGetValue(
                client.Id,
                out var request);

            if (!client.IsAuthorized && !hasPendingRequest)
                continue;

            Guid requestId = hasPendingRequest
                ? request.RequestId
                : Guid.Empty;

            var computer = new ComputerDto(
                requestId == Guid.Empty ? client.Id : requestId,
                client.IpAddress,
                client.HostName,
                true);

            AddRow(computer, requestId, client.IsAuthorized);
        }
    }

    //private void Subscribe(TcpServer server)
    //{
    //    Debug.Log("Subscribe");
    //    _server.AdminActionRequestReceived += OnRequestReceived;
    //    _server.ClientDisconnected += OnClientDisconnected;
    //}

    //private void Unsubscribe()
    //{
    //    if (_server == null)
    //        return;
    //    Debug.Log("Unsubscribe");
    //    _server.AdminActionRequestReceived -= OnRequestReceived;
    //    _server.ClientDisconnected -= OnClientDisconnected;
    //    _server = null;
    //}

    private void AddRow(ComputerDto computer, Guid requestId, bool isAuthorized)
    {
        var row = _view.CreateRow();

        _rows[row] = (computer, requestId);

        row.SetIpAddress(computer.IpAddress);
        row.SetHostName(computer.HostName);

        row.ButtonActive(!isAuthorized);

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
        //Debug.Log(
        //    $"Disconnect: {computer.IpAddress}");

        ////_service.Disconnect(computer.IpAddress);

        //Show();
    }

    private void OnRequestReceived(PendingRequest request)
    {
        if (request.Contract != nameof(AccessRequest))
            return;

        Debug.Log("OnRequestReceived");
        UpdateList();
    }

    private void OnClientDisconnected(Guid clientId)
    {
        Debug.Log("DISCONNNECTEED");
        UpdateList();
    }

    public void Dispose()
    {

        //Unsubscribe();
    }
}
