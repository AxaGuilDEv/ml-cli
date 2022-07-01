using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ml.Cli.WebApp.Server.Audits.Database;


[Table("T_Audit", Schema = "sch_ECOTAG")]
public class AuditModel
{
    [Key] 
    [Column("AUD_Id")] 
    public Guid Id { get; set; }
    
    [Column("AUD_ElementId")] 
    public Guid ElementId { get; set; }
     
    [Column("AUD_Type")] 
    public string Type { get; set; }
    
    [Column("AUD_Diff")] 
    public string Diff { get; set; }
    
    [Column("AUD_NameIdentifier")]
    [MaxLength(64)]
    [MinLength(1)]
    public string NameIdentifier { get; set; }

    [Column("AUD_CreateDate")] 
    public long CreateDate { get; set; }
    
}
