using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ml.Cli.WebApp.Server.Datasets.Database;

[Table("T_Dataset", Schema = "sch_ECOTAG")]
public class DatasetModel
{
    [Key] [Column("DTS_Id")] public Guid Id { get; set; }

    [Column("DTS_Name")]
    [MaxLength(48)]
    [MinLength(3)]
    public string Name { get; set; }
    
    [Column("DTS_BlobUri")]
    [MaxLength(512)]
    [MinLength(1)]
    
    public string BlobUri { get; set; }

    [Column("DTS_Type")] public DatasetTypeEnumeration Type { get; set; }

    [Column("DTS_Classification")] public DatasetClassificationEnumeration Classification { get; set; }

    [Column("GRP_Id")] public Guid GroupId { get; set; }

    [Column("DTS_CreatorNameIdentifier")]
    [MaxLength(64)]
    [MinLength(1)]
    public string CreatorNameIdentifier { get; set; }

    [Column("DTS_CreateDate")] public long CreateDate { get; set; }

    [Column("DTS_Locked")] public DatasetLockedEnumeration Locked { get; set; }

    public List<FileModel> Files { get; set; }
    
}