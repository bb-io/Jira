using System.Text.RegularExpressions;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Apps.Jira.Utils;

public static class MarkdownToJiraConverter
{
    public static object ConvertMarkdownToJiraDoc(string markdownContent)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        var markdownDocument = Markdown.Parse(markdownContent, pipeline);
        var contentList = new List<object>();

        foreach (var block in markdownDocument)
        {
            var contentElement = ProcessBlock(block);
            if (contentElement != null)
                contentList.Add(contentElement);
        }

        return new
        {
            type = "doc",
            version = 1,
            content = contentList
        };
    }

    private static object ProcessBlock(Block block)
    {
        switch (block)
        {
            case ParagraphBlock paragraphBlock:
                var inline = paragraphBlock.Inline?.FirstChild;
                var paragraphContent = ProcessInlines(ref inline);
                paragraphContent = CombineTextNodes(paragraphContent);
                return new
                {
                    type = "paragraph",
                    content = paragraphContent
                };

            case ListBlock listBlock:
                var listContent = new List<object>();
                foreach (var listItem in listBlock)
                {
                    var itemContent = ProcessBlock(listItem);
                    if (itemContent != null)
                        listContent.Add(itemContent);
                }
                return new
                {
                    type = listBlock.IsOrdered ? "orderedList" : "bulletList",
                    content = listContent
                };

            case ListItemBlock listItemBlock:
                var listItemContent = new List<object>();
                foreach (var subBlock in listItemBlock)
                {
                    var contentElement = ProcessBlock(subBlock);
                    if (contentElement != null)
                        listItemContent.Add(contentElement);
                }
                return new
                {
                    type = "listItem",
                    content = listItemContent
                };

            case HeadingBlock headingBlock:
                var headingInline = headingBlock.Inline?.FirstChild;
                var headingContent = ProcessInlines(ref headingInline);
                headingContent = CombineTextNodes(headingContent);
                return new
                {
                    type = "heading",
                    attrs = new { level = headingBlock.Level },
                    content = headingContent
                };

            default:
                return null;
        }
    }

    private static List<object> ProcessInlines(ref Inline inline, List<Dictionary<string, object>> currentMarks = null)
    {
        var result = new List<object>();

        while (inline != null)
        {
            var content = ProcessInline(ref inline, currentMarks);
            if (content != null)
                result.AddRange(content);
        }

        return result;
    }

    private static List<object> ProcessInline(ref Inline inline, List<Dictionary<string, object>> currentMarks = null)
    {
        var result = new List<object>();

        switch (inline)
        {
            case LiteralInline literalInline:
                var text = literalInline.Content.ToString();
                var parts = Regex.Split(text, @"(\[Attachment:\s*.*?\])");

                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(part))
                        continue;

                    var attachmentMatch = Regex.Match(part, @"^\[Attachment:\s*(.*?)\]$");
                    if (attachmentMatch.Success)
                    {
                        var attachmentId = attachmentMatch.Groups[1].Value;
                        var attachmentElement = new Dictionary<string, object>
                        {
                            { "type", "mediaInline" },
                            { "attrs", new Dictionary<string, object>
                                {
                                    { "type", "file" },
                                    { "id", attachmentId },
                                    { "collection", "" }
                                }
                            }
                        };

                        if (currentMarks != null && currentMarks.Any())
                        {
                            attachmentElement["marks"] = currentMarks;
                        }

                        result.Add(attachmentElement);
                    }
                    else
                    {
                        var textElement = new Dictionary<string, object>
                        {
                            { "type", "text" },
                            { "text", part }
                        };

                        if (currentMarks != null && currentMarks.Any())
                        {
                            textElement["marks"] = currentMarks;
                        }

                        result.Add(textElement);
                    }
                }
                inline = inline.NextSibling;
                break;

            case EmphasisInline emphasisInline:
                var marks = new List<Dictionary<string, object>>(currentMarks ?? new List<Dictionary<string, object>>());
                var markType = emphasisInline.DelimiterCount == 2 ? "strong" : "em";
                marks.Add(new Dictionary<string, object> { { "type", markType } });

                var childInline = emphasisInline.FirstChild;
                var emphasisContent = ProcessInlines(ref childInline, marks);
                result.AddRange(emphasisContent);
                // Advance to next sibling
                inline = inline.NextSibling;
                break;

            case LinkInline linkInline:
                var linkMarks = new List<Dictionary<string, object>>(currentMarks ?? new List<Dictionary<string, object>>())
                {
                    new ()
                    {
                        { "type", "link" },
                        { "attrs", new Dictionary<string, object> { { "href", linkInline.Url } } }
                    }
                };

                var linkChildInline = linkInline.FirstChild;
                var linkContent = ProcessInlines(ref linkChildInline, linkMarks);
                result.AddRange(linkContent);
                inline = inline.NextSibling;
                break;

            case HtmlInline htmlInline:
                var html = htmlInline.Tag;
                if (html.StartsWith("<span") && html.Contains("style"))
                {
                    var styleMatch = Regex.Match(html, "style\\s*=\\s*\"([^\"]*)\"");
                    if (styleMatch.Success)
                    {
                        var style = styleMatch.Groups[1].Value;
                        var colorMatch = Regex.Match(style, "color\\s*:\\s*(#[0-9a-fA-F]{3,6}|[a-zA-Z]+);?");
                        if (colorMatch.Success)
                        {
                            var color = colorMatch.Groups[1].Value;
                            var colorMark = new Dictionary<string, object>
                            {
                                { "type", "textColor" },
                                { "attrs", new Dictionary<string, object> { { "color", color } } }
                            };
                            var marksWithColor = new List<Dictionary<string, object>>(currentMarks ?? new List<Dictionary<string, object>>())
                            {
                                colorMark
                            };

                            // Process inlines until the closing </span>
                            var contentList = new List<object>();
                            inline = inline.NextSibling;
                            while (inline != null && !(inline is HtmlInline endSpan && endSpan.Tag == "</span>"))
                            {
                                var innerContent = ProcessInline(ref inline, marksWithColor);
                                if (innerContent != null)
                                {
                                    contentList.AddRange(innerContent);
                                }
                            }

                            // Skip the closing </span>
                            if (inline is HtmlInline)
                            {
                                inline = inline.NextSibling;
                            }

                            result.AddRange(contentList);
                            break;
                        }
                    }
                }
                // If not a span with style, advance inline
                inline = inline.NextSibling;
                break;

            default:
                if (inline is ContainerInline containerInline)
                {
                    var firstChildInline = containerInline.FirstChild;
                    var containerContent = ProcessInlines(ref firstChildInline, currentMarks);
                    result.AddRange(containerContent);
                }
                // Advance to next sibling
                inline = inline.NextSibling;
                break;
        }

        return result;
    }

    private static List<object> CombineTextNodes(List<object> nodes)
    {
        var combinedNodes = new List<object>();
        IDictionary<string, object> previousTextNode = null;

        foreach (var node in nodes)
        {
            if (node is IDictionary<string, object> currentNode && currentNode["type"] as string == "text")
            {
                if (previousTextNode != null)
                {
                    var prevMarks = previousTextNode.ContainsKey("marks") ? previousTextNode["marks"] as List<Dictionary<string, object>> : null;
                    var currentMarks = currentNode.ContainsKey("marks") ? currentNode["marks"] as List<Dictionary<string, object>> : null;

                    if (MarksAreEqual(prevMarks, currentMarks))
                    {
                        previousTextNode["text"] = previousTextNode["text"].ToString() + currentNode["text"];
                        continue;
                    }
                }
                combinedNodes.Add(currentNode);
                previousTextNode = currentNode;
            }
            else
            {
                combinedNodes.Add(node);
                previousTextNode = null;
            }
        }

        return combinedNodes;
    }

    private static bool MarksAreEqual(List<Dictionary<string, object>> marks1, List<Dictionary<string, object>> marks2)
    {
        if (marks1 == null && marks2 == null) return true;
        if (marks1 == null || marks2 == null) return false;
        if (marks1.Count != marks2.Count) return false;

        for (int i = 0; i < marks1.Count; i++)
        {
            var mark1 = marks1[i];
            var mark2 = marks2[i];
            if (!mark1["type"].Equals(mark2["type"])) return false;

            if (mark1.ContainsKey("attrs") || mark2.ContainsKey("attrs"))
            {
                if (!mark1.ContainsKey("attrs") || !mark2.ContainsKey("attrs")) return false;
                var attrs1 = mark1["attrs"] as IDictionary<string, object>;
                var attrs2 = mark2["attrs"] as IDictionary<string, object>;
                if (!AttrsAreEqual(attrs1, attrs2)) return false;
            }
        }

        return true;
    }

    private static bool AttrsAreEqual(IDictionary<string, object> attrs1, IDictionary<string, object> attrs2)
    {
        if (attrs1 == null && attrs2 == null) return true;
        if (attrs1 == null || attrs2 == null) return false;
        if (attrs1.Count != attrs2.Count) return false;

        foreach (var key in attrs1.Keys)
        {
            if (!attrs2.ContainsKey(key) || !attrs1[key].Equals(attrs2[key])) return false;
        }

        return true;
    }
}

