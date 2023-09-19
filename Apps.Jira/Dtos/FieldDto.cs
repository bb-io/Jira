namespace Apps.Jira.Dtos;

public class FieldDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool Custom { get; set; }
    public SchemaDto? Schema { get; set; }
}

public class SchemaDto 
{
    public string Type { get; set; }
    public string? Custom { get; set; }
}