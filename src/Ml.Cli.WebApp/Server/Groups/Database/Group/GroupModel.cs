
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ml.Cli.WebApp.Server.Groups.Database.GroupUsers;

namespace Ml.Cli.WebApp.Server.Groups.Database.Group;

[Table("T_Group", Schema = "sch_ECOTAG")]
public class GroupModel
{
    [Key]
    [Column("GRP_Id")]
    public Guid Id { get; set; }
    
    [Column("GRP_Name")]
    [MaxLength(48)]
    [MinLength(3)]
    public string Name { get; set; }
    
    [Column("GRP_CreatorNameIdentifier")]
    [MaxLength(64)]
    [MinLength(1)]
    public string CreatorNameIdentifier { get; set; }
    
    [Column("GRP_CreateDate")]
    public long CreateDate { get; set; }
    
    [Column("GRP_UpdateDate")]
    public long UpdateDate { get; set; }
    
    public List<GroupUsersModel> GroupUsers { get; set; }
}