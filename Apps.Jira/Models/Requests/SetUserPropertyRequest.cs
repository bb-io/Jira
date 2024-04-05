using Blackbird.Applications.Sdk.Common;

namespace Apps.Jira.Models.Requests;

public class SetUserPropertyRequest
{
    [Display("Boolean value", Description = "The boolean value to set.")]
    public bool? BooleanValue { get; set; }
    
    [Display("String value", Description = "The string value to set.")]
    public string? StringValue { get; set; }
    
    [Display("Integer value", Description = "The integer value to set.")]
    public int? IntegerValue { get; set; }
    
    [Display("Date value", Description = "The date value to set.")]
    public DateTime? DateValue { get; set; }
    
    [Display("Array value", Description = "The array value to set.")]
    public string[]? ArrayValue { get; set; }
}