﻿using System;

namespace Ml.Cli.WebApp.Server.Projects.Database.Project;

public static class Converter
{
    public static ProjectDataModel ToProjectDataModel(this ProjectModel projectModel)
    {
        return new ProjectDataModel
        {
            Id = projectModel.Id.ToString(),
            DatasetId = projectModel.DatasetId.ToString(),
            GroupId = projectModel.GroupId.ToString(),
            LabelsJson = projectModel.LabelsJson,
            Name = projectModel.Name,
            AnnotationType = projectModel.AnnotationType.ToString(),
            CreateDate = projectModel.CreateDate,
            NumberCrossAnnotation = projectModel.NumberCrossAnnotation,
            CreatorNameIdentifier = projectModel.CreatorNameIdentifier
        };
    }

    public static AnnotationTypeEnumeration ToAnnotationType(this string type)
    {
        return (AnnotationTypeEnumeration)Enum.Parse(typeof(AnnotationTypeEnumeration), type);
    }
}