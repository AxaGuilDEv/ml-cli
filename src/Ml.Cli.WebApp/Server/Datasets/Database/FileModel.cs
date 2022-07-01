using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ml.Cli.WebApp.Server.Datasets.Database.Annotations;
using Ml.Cli.WebApp.Server.Projects;

namespace Ml.Cli.WebApp.Server.Datasets.Database;

[Table("T_File", Schema = "sch_ECOTAG")]
public class FileModel
{
    [Key] [Column("FLE_Id")] public Guid Id { get; set; }

    [Column("FLE_Name")]
    [MaxLength(512)]
    [MinLength(1)]
    public string Name { get; set; }

    [Column("FLE_Size")] public long Size { get; set; }

    [Column("FLE_ContentType")]
    [MaxLength(16)]
    [MinLength(3)]
    public string ContentType { get; set; }

    [Column("FLE_CreatorNameIdentifier")]
    [MaxLength(64)]
    [MinLength(1)]
    public string CreatorNameIdentifier { get; set; }

    [Column("FLE_CreateDate")] public long CreateDate { get; set; }

    [Column("DTS_Id")] public Guid DatasetId { get; set; }

    public DatasetModel Dataset { get; set; }
    
    public IList<AnnotationModel> Annotations { get; set; }
    public IList<ReservationModel> Reservations { get; set; }
}