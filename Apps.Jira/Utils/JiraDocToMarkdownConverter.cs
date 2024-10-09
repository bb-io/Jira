using System.Net.Mail;
using System.Text;
using Apps.Jira.Dtos;

namespace Apps.Jira.Utils;

public static class JiraDocToMarkdownConverter
{
    public static string ConvertToMarkdown(Description description)
    {
        var markdown = new StringBuilder();
        foreach (var element in description.Content)
            ProcessContentElement(element, markdown, 0);
        return markdown.ToString();
    }

    private static void ProcessContentElement(ContentElement element, StringBuilder markdown, int indentLevel)
    {
        switch (element.Type)
        {
            case "heading":
                int level = 1;
                if (element.Attrs != null && element.Attrs.ContainsKey("level"))
                {
                    level = Convert.ToInt32(element.Attrs["level"]);
                }
                string hashes = new string('#', level);
                markdown.Append(hashes + " ");
                if (element.Content != null)
                {
                    foreach (var content in element.Content)
                        ProcessContentElement(content, markdown, indentLevel);
                }
                markdown.AppendLine("\n");
                break;

            case "paragraph":
                foreach (var content in element.Content)
                    ProcessContentElement(content, markdown, indentLevel);
                markdown.AppendLine("\n");
                break;

            case "text":
                string text = element.Text;
                if (element.Marks != null && element.Marks.Any())
                {
                    var linkMark = element.Marks.FirstOrDefault(m => m.Type == "link");
                    if (linkMark != null)
                    {
                        var href = linkMark.Attrs?.ContainsKey("href") == true ? linkMark.Attrs["href"].ToString() : "#";
                        text = $"[{text}]({href})";
                    }
                    foreach (var mark in element.Marks.Where(m => m.Type != "link"))
                    {
                        switch (mark.Type)
                        {
                            case "strong":
                                text = $"**{text}**";
                                break;
                            case "em":
                                text = $"*{text}*";
                                break;
                            case "textColor":
                                if (mark.Attrs != null && mark.Attrs.ContainsKey("color"))
                                {
                                    var color = mark.Attrs["color"].ToString();
                                    text = $"<span style=\"color:{color};\">{text}</span>";
                                }
                                break;
                        }
                    }
                }
                markdown.Append(text);
                break;

            case "bulletList":
                foreach (var listItem in element.Content)
                    ProcessContentElement(listItem, markdown, indentLevel);
                markdown.AppendLine();
                break;

            case "listItem":
                foreach (var content in element.Content)
                {
                    if (content.Type == "paragraph")
                    {
                        markdown.Append(new string(' ', indentLevel * 2) + "- ");
                        ProcessContentElement(content, markdown, indentLevel);
                        markdown.AppendLine();
                    }
                    else if (content.Type == "bulletList")
                    {
                        ProcessContentElement(content, markdown, indentLevel + 1);
                    }
                    else
                    {
                        ProcessContentElement(content, markdown, indentLevel);
                    }
                }
                break;

            case "mediaInline":
            case "mediaSingle":
            case "media":
                if (element.Attrs != null && element.Attrs.ContainsKey("id"))
                {
                    var mediaId = element.Attrs["id"]!.ToString();
                    markdown.Append($"[Attachment: {mediaId}]");
                }
                break;

            default:
                if (element.Content != null)
                {
                    foreach (var content in element.Content)
                        ProcessContentElement(content, markdown, indentLevel);
                }
                break;
        }
    }
}
