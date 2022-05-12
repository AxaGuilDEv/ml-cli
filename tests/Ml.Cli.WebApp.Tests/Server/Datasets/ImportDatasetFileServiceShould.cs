﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ml.Cli.WebApp.Server;
using Ml.Cli.WebApp.Server.Datasets.Database;
using Ml.Cli.WebApp.Server.Datasets.Database.FileStorage;
using Moq;
using Xunit;

namespace Ml.Cli.WebApp.Tests.Server.Datasets;

public class ImportDatasetFileServiceShould
{
    [Fact]
    public async Task Should_Import_Files()
    {
        var (fileService, datasetContext, createDataset, datasetModel, importDatasetFileService) = InitMockAsync();
        await importDatasetFileService.ImportDatasetFiles(fileService, datasetContext, createDataset, datasetModel);
        Assert.Equal(1, datasetContext.Files.Count());
    }

    private static (IFileService Object, DatasetContext datasetContext, CreateDataset createDataset, DatasetModel datasetModel, ImportDatasetFilesService importDatasetFileService) InitMockAsync()
    {
        var filesDict = new Dictionary<string, ResultWithError<FileInfoServiceDataModel, ErrorResult>>
        {
            {
                "firstFile.txt",
                new ResultWithError<FileInfoServiceDataModel, ErrorResult>
                    { Error = new ErrorResult { Key = FileService.InvalidFileExtension } }
            },
            {
                "secondFile.txt",
                new ResultWithError<FileInfoServiceDataModel, ErrorResult>
                {
                    Data = new FileInfoServiceDataModel
                        { Name = "secondFile.txt", Length = 10, ContentType = "image" }
                }
            },
            { "thirdFile.txt", new ResultWithError<FileInfoServiceDataModel, ErrorResult>{Error = new ErrorResult{Key = FileService.InvalidFileExtension}} }
        };
        var fileService = new Mock<IFileService>();
        fileService
            .Setup(foo =>
                foo.GetInputDatasetFilesAsync("TransferFileStorage", "input", "groupName/datasetName", It.IsAny<string>()))
            .ReturnsAsync(filesDict);
        var datasetContext = DatasetMock.GetInMemoryDatasetContext()();
        var createDataset = new CreateDataset
        {
            CreatorNameIdentifier = "s666666",
            ImportedDatasetName = "groupName/datasetName"
        };
        var datasetModel = new DatasetModel
        {
            Id = new Guid(),
            Type = DatasetTypeEnumeration.Image,
        };
        var importDatasetFileService = new ImportDatasetFilesService(null);
        return (fileService.Object, datasetContext, createDataset, datasetModel, importDatasetFileService);
    }
}