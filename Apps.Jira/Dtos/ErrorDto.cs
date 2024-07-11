using Newtonsoft.Json.Linq;

namespace Apps.Jira.Dtos;

public class ErrorDto
{
    public IEnumerable<string> ErrorMessages { get; set; }
    public JObject Errors { get; set; }
}