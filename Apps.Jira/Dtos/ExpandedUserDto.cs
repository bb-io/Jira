using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Dtos;

public class ExpandedUserDto :  Apps.Jira.Models.Responses.UserDto
{
    public string TimeZone { get; set; }
    
    public string Locale { get; set; }
    
    public GroupsDto Groups { get; set; }
    
    public ApplicationRolesDto ApplicationRoles { get; set; }
    
    public string Expand { get; set; }
}

public class GroupsDto
{
    public int Size { get; set; }
    
    public UserGroupDto[] Items { get; set; }
}

public class UserGroupDto
{
    [Display("Group ID")]
    public string GroupId { get; set; }
    
    public string Name { get; set; }
    
    public string Self { get; set; }
}

public class ApplicationRolesDto
{
    public int Size { get; set; }
    
    public ApplicationRole[] Items { get; set; }
}
 
 public class ApplicationRole
 {
     public string[] DefaultGroups { get; set; }

     public bool Defined { get; set; }

     [Display("Has unlimited seats")]
     public bool HasUnlimitedSeats { get; set; }
     
     public string Key { get; set; }
     
     public string Name { get; set; }
     
     public bool Platform { get; set; }
 }