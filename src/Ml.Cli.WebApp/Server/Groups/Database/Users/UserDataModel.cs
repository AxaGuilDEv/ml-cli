using System.Collections.Generic;

namespace Ml.Cli.WebApp.Server.Groups.Database.Users;

public class UserDataModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string NameIdentifier { get; set; }
}