namespace Apps.Jira.Webhooks.Payload;

public class Changelog
{
    public IEnumerable<Item> Items { get; set; }
}

public class Item
{
    public string Field { get; set; }
    public string FieldId { get; set; }
    public string From { get; set; }
    public string FromString { get; set; }
    public string To { get; set; }
    public string ToString { get; set; }
}