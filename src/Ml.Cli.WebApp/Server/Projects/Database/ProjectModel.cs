using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ml.Cli.WebApp.Server.Projects.Database;

[Table("T_Project", Schema = "sch_ECOTAG")]
public class ProjectModel
{
    [Key] [Column("PRJ_Id")] public Guid Id { get; set; }

    [Column("DTS_Id")] public Guid DatasetId { get; set; }

    [Column("GRP_Id")] public Guid GroupId { get; set; }

    [Column("PRJ_Name")]
    [MaxLength(48)]
    [MinLength(3)]
    public string Name { get; set; }

    [Column("PRJ_NumberCrossAnnotation")] public int NumberCrossAnnotation { get; set; }

    [Column("PRJ_CreateDate")] public long CreateDate { get; set; }

    [Column("PRJ_AnnotationType")] public AnnotationTypeEnumeration AnnotationType { get; set; }

    [Column("PRJ_LabelsJson")]
    [MaxLength(2048)]
    public string LabelsJson { get; set; }

    [Column("PRJ_CreatorNameIdentifier")]
    [MaxLength(64)]
    [MinLength(1)]
    public string CreatorNameIdentifier { get; set; }
}