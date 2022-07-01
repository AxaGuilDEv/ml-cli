using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ml.Cli.WebApp.Server.Datasets;
using Ml.Cli.WebApp.Server.Datasets.Cmd;
using Ml.Cli.WebApp.Server.Datasets.Database.FileStorage;
using Moq;
using Xunit;

namespace Ml.Cli.WebApp.Tests.Server.Datasets;

public class GetImportedDatasetShould
{
    [Theory]
    [InlineData("s666666", 2)]
    [InlineData("s111111", 0)]
    public async Task GetImportedDatasets(string nameIdentifier, int nbDatasets)
    {
        var mockResult = await DatasetMock.InitMockAsync(nameIdentifier);
        var mockedTransferService = new Mock<IFileService>();
        mockedTransferService.Setup(foo => foo.GetImportedDatasetsNamesAsync("azureblob://TransferFileStorage/input"))
            .ReturnsAsync(new List<string> { "group2/test1", "group2/test2", "group1/demo" });
        
        var options = Options.Create(new DatasetsSettings() { IsBlobTransferActive = true});
        var getImportedDatasetsCmd =
            new GetImportedDatasetsCmd(mockResult.UsersRepository, mockedTransferService.Object, mockResult.DatasetsRepository, options);
        var datasets = await mockResult.DatasetsController.GetImportedDatasets(getImportedDatasetsCmd);
        
        Assert.Equal(nbDatasets, datasets.Count);
    }
}