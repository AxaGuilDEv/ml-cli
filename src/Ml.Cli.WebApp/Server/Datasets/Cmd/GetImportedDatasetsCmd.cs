using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ml.Cli.WebApp.Server.Datasets.Database;
using Ml.Cli.WebApp.Server.Datasets.Database.FileStorage;
using Ml.Cli.WebApp.Server.Groups.Database.Users;

namespace Ml.Cli.WebApp.Server.Datasets.Cmd;

public class GetImportedDatasetsCmd
{
    private readonly UsersRepository _usersRepository;
    private readonly IFileService _fileService;
    private readonly DatasetsRepository _datasetsRepository;
    private readonly IOptions<DatasetsSettings> _datasetsSettings;

    public GetImportedDatasetsCmd(UsersRepository usersRepository, IFileService fileService, DatasetsRepository datasetsRepository, IOptions<DatasetsSettings> datasetsSettings)
    {
        _datasetsRepository = datasetsRepository;
        _datasetsSettings = datasetsSettings;
        _usersRepository = usersRepository;
        _fileService = fileService;
    }

    public async Task<IList<string>> ExecuteAsync(string nameIdentifier)
    {
        if(!_datasetsSettings.Value.IsBlobTransferActive)
        {
            return new List<string>();
        }

        var user = await _usersRepository.GetUserByNameIdentifierWithGroupIdsAsync(nameIdentifier);
        if (user == null)
        {
            return new List<string>();
        }

        var datasetsNames = await _fileService.GetImportedDatasetsNamesAsync("azureblob://TransferFileStorage/input");
        var datasets = await _datasetsRepository.ListDatasetAsync(null, user.GroupIds);
        if (datasets == null)
        {
            return datasetsNames;
        }

        return datasetsNames.Where(datasetsName => datasets.Count(d => d.BlobUri.EndsWith($"/{datasetsName}")) == 0).ToList();
    }
}