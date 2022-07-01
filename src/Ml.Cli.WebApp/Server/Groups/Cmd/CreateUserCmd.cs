using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Ml.Cli.WebApp.Server.Groups.Database.Users;
using Ml.Cli.WebApp.Server.Groups.Oidc;

namespace Ml.Cli.WebApp.Server.Groups.Cmd;

public record CreateUserInput
{
    [Required]
    [MaxLength(64)]
    public string NameIdentifier { get; set; }
    
    [Required]
    public string AccessToken { get; set; }
}

public class CreateUserCmd
{
    private readonly UsersRepository _userRepository;
    private readonly IOidcUserInfoService _userInfoService;
    public const string InvalidModel = "InvalidModel";

    public CreateUserCmd(UsersRepository userRepository, IOidcUserInfoService userInfoService)
    {
        _userRepository = userRepository;
        _userInfoService = userInfoService;
    }

    public async Task<ResultWithError<string, ErrorResult>> ExecuteAsync(CreateUserInput createUserInput)
    {
        var commandResult = new ResultWithError<string, ErrorResult>();

        var validationResult = new Validation().Validate(createUserInput);
        if (!validationResult.IsSuccess)
        {
            commandResult.Error = new ErrorResult
            {
                Key = InvalidModel,
                Error = validationResult.Errors
            };
            return commandResult;
        }
        var nameIdentifier = createUserInput.NameIdentifier;
        var accessToken = createUserInput.AccessToken;
        var userDataModel = await _userRepository.GetUserByNameIdentifierAsync(nameIdentifier);
        if (userDataModel != null) return commandResult;
        var userEMail = await _userInfoService.GetUserEmailAsync(accessToken);
        var result = await _userRepository.CreateUserAsync(userEMail.Email, nameIdentifier);
        if (!result.IsSuccess)
        {
            commandResult.Error = result.Error;
            return commandResult;
        }
        commandResult.Data = result.Data;

        return commandResult;
    }
}