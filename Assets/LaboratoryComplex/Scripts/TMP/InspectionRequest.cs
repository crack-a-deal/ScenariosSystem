
using Networking.Request;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class Register3DModelViewRequest
    : IRequest<Register3DModelViewResponse>
{
    public Guid UserProfileId { get; set; }
    public string ModelName { get; set; } = string.Empty;
}

public class Register3DModelViewResponse : IResponse
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class Register3DModelViewHandler
    : IRequestHandler<Register3DModelViewRequest, Register3DModelViewResponse>
{
    private readonly IThreeDModelService _service;

    public Register3DModelViewHandler(IThreeDModelService service)
    {
        _service = service;
    }

    public async Task<Register3DModelViewResponse> HandleAsync(
        Register3DModelViewRequest request)
    {
        try
        {
            await _service.RegisterViewAsync(
                request.UserProfileId,
                request.ModelName);

            return new Register3DModelViewResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to register model view: {ex}");

            return new Register3DModelViewResponse
            {
                Success = false,
                Error = "Не удалось сохранить просмотр модели."
            };
        }
    }
}
