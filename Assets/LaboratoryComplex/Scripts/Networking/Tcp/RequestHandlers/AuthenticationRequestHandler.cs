using Networking.Request;
using System.Threading.Tasks;
using UnityEngine;

public class AuthenticationRequestHandler : IRequestHandler<AuthenticationRequest, AuthenticationResponse>
{
    private readonly IValidateUsersService _validateService;
    private readonly IUserProfileService _userProfileService;

    public AuthenticationRequestHandler(IValidateUsersService validateService,IUserProfileService userProfileService)
    {
        _validateService = validateService;
        _userProfileService = userProfileService;
    }

    public async Task<AuthenticationResponse> HandleAsync(AuthenticationRequest request)
    {
        var validateData = await _validateService.ValidateUserAsync(request.Login, request.Password);
        var profile = await _userProfileService.GetUserProfileIdByAccountid(validateData.Item2);

        if(profile == null)
        {
            Debug.Log("NOT FOUND");
        }

        return new AuthenticationResponse
        {
            AccountId=validateData.Item2,
            ProfileId = profile.Id,
            Success = validateData.Item1,
            ErrorMessage = validateData.Item1 ? null : "Неверный логин или пароль"
        };
    }
}