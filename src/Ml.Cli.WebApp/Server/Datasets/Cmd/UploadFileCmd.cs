using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ml.Cli.WebApp.Server.Datasets.Database;
using Ml.Cli.WebApp.Server.Groups.Database.Users;

namespace Ml.Cli.WebApp.Server.Datasets.Cmd;

public record UploadFile
{
    [Required]
    [MaxLength(1024)]
    [RegularExpression(@"^[a-zA-Z0-9-_\. (){}-]*$")]
    [MinLength(3)]
    public string Name { get; set; }

    [Required]
    [MaxLength(256)]
    [MinLength(3)]
    public string ContentType { get; set; }

    [Required] public Stream Stream { get; set; }
}

public record UploadFileCmdInput
{
    [Required] public IList<UploadFile> Files { get; set; }

    [Required]
    [RegularExpression("(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$")]
    public string DatasetId { get; set; }

    [MaxLength(64)] [MinLength(1)] public string NameIdentifier { get; set; }
}

public class UploadFileCmd
{
    public const string FileTooLarge = "FileTooLarge";

    public const string DatasetLocked = "DatasetLocked";
    public const string UserNotInGroup = "UserNotInGroup";
    public const string UserNotFound = "UserNotFound";
    public const string InvalidModel = "InvalidModel";
    
    private readonly DatasetsRepository _datasetsRepository;
    private readonly UsersRepository _usersRepository;

    public UploadFileCmd(UsersRepository usersRepository, DatasetsRepository datasetsRepository)
    {
        _usersRepository = usersRepository;
        _datasetsRepository = datasetsRepository;
    }

    public async Task<ResultWithError<IList<ResultWithError<string, ErrorResult>>, ErrorResult>> ExecuteAsync(
        UploadFileCmdInput uploadFileCmdInput)
    {
        var commandResult = new ResultWithError<IList<ResultWithError<string, ErrorResult>>, ErrorResult>();

        var validationResult = new Validation().Validate(uploadFileCmdInput);
        if (!validationResult.IsSuccess) return commandResult.ReturnError(InvalidModel, validationResult.Errors);

        var nameIdentifier = uploadFileCmdInput.NameIdentifier;
        var user = await _usersRepository.GetUserByNameIdentifierWithGroupIdsAsync(nameIdentifier);
        if (user == null) return commandResult.ReturnError(UserNotFound);

        var datasetId = uploadFileCmdInput.DatasetId;
        var datasetInfo = await _datasetsRepository.GetDatasetInfoAsync(datasetId);

        foreach (var file in uploadFileCmdInput.Files)
        {
            var fileValidationResult = new Validation().Validate(file);
            if (!fileValidationResult.IsSuccess)
                return commandResult.ReturnError(InvalidModel, validationResult.Errors);

            if (!FileValidator.IsFileExtensionValid(file.Name, datasetInfo.Type)) return commandResult.ReturnError(InvalidModel);

            if (FileValidator.IsFileSizeValid(file.Stream)) continue;
            return commandResult.ReturnError(FileTooLarge);
        }

        if (datasetInfo.Locked != DatasetLockedEnumeration.None)
        {
            commandResult.Error = new ErrorResult
            {
                Key = DatasetLocked
            };
            return commandResult;
        }

        if (!user.GroupIds.Contains(datasetInfo.GroupId))
        {
            commandResult.Error = new ErrorResult
            {
                Key = UserNotInGroup
            };
            return commandResult;
        }

        var results = (from file in uploadFileCmdInput.Files
                let fileName = file.Name
                let stream = file.Stream
                select _datasetsRepository.CreateFileAsync(datasetId, stream, fileName, file.ContentType,
                    nameIdentifier))
            .ToList();
        Task.WaitAll(results.ToArray());
        commandResult.Data = results.Select(r => r.Result).ToList();
        return commandResult;
    }
}