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
                var paragraphContent = new List<object>();
                foreach (var inline in paragraphBlock.Inline)
                {
                    var content = ProcessInline(inline);
                    if (content != null)
                        AddContentToList(paragraphContent, content);
                }
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
                bool hasParagraph = false;
                foreach (var subBlock in listItemBlock)
                {
                    var contentElement = ProcessBlock(subBlock);
                    if (contentElement != null)
                    {
                        if (((dynamic)contentElement).type == "paragraph")
                            hasParagraph = true;
                        listItemContent.Add(contentElement);
                    }
                }
                if (!hasParagraph)
                {
                    listItemContent.Insert(0, new
                    {
                        type = "paragraph",
                        content = new List<object>()
                    });
                }
                return new
                {
                    type = "listItem",
                    content = listItemContent
                };

            case HeadingBlock headingBlock:
                var headingContent = new List<object>();
                foreach (var inline in headingBlock.Inline)
                {
                    var content = ProcessInline(inline);
                    if (content != null)
                    {
                        ApplyMarkToContent(content, "strong");
                        AddContentToList(headingContent, content);
                    }
                }
                headingContent = CombineTextNodes(headingContent);
                return new
                {
                    type = "paragraph",
                    content = headingContent
                };

            default:
                return null;
        }
    }

    private static List<object> ProcessInline(Inline inline)
    {
        var result = new List<object>();

        switch (inline)
        {
            case LiteralInline literalInline:
                result.Add(new Dictionary<string, object>
                {
                    { "type", "text" },
                    { "text", literalInline.Content.ToString() }
                });
                break;

            case EmphasisInline emphasisInline:
                var emphasisContent = new List<object>();
                foreach (var childInline in emphasisInline)
                {
                    var content = ProcessInline(childInline);
                    if (content != null)
                    {
                        foreach (var item in content)
                        {
                            ApplyMarkToContent(item, emphasisInline.DelimiterCount == 2 ? "strong" : "em");
                        }
                        emphasisContent.AddRange(content);
                    }
                }
                result.AddRange(emphasisContent);
                break;

            case LinkInline linkInline:
                var linkContent = new List<object>();
                foreach (var childInline in linkInline)
                {
                    var content = ProcessInline(childInline);
                    if (content != null)
                    {
                        foreach (var item in content)
                        {
                            ApplyLinkMarkToContent(item, linkInline.Url);
                        }
                        linkContent.AddRange(content);
                    }
                }
                result.AddRange(linkContent);
                break;

            default:
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
                    var prevMarks = previousTextNode.ContainsKey("marks") ? previousTextNode["marks"] as List<object> : null;
                    var currentMarks = currentNode.ContainsKey("marks") ? currentNode["marks"] as List<object> : null;

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

    private static bool MarksAreEqual(List<object> marks1, List<object> marks2)
    {
        if (marks1 == null && marks2 == null) return true;
        if (marks1 == null || marks2 == null) return false;
        if (marks1.Count != marks2.Count) return false;

        for (int i = 0; i < marks1.Count; i++)
        {
            var mark1 = marks1[i] as IDictionary<string, object>;
            var mark2 = marks2[i] as IDictionary<string, object>;
            if (mark1["type"] as string != mark2["type"] as string) return false;

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

    private static void ApplyMarkToContent(object content, string markType)
    {
        if (content is IDictionary<string, object> contentDict)
        {
            if (!contentDict.TryGetValue("marks", out var marksObj))
            {
                marksObj = new List<object>();
                contentDict["marks"] = marksObj;
            }
            var marks = (List<object>)marksObj;
            marks.Add(new Dictionary<string, object> { { "type", markType } });
        }
        else if (content is List<object> contentList)
        {
            foreach (var item in contentList)
                ApplyMarkToContent(item, markType);
        }
    }

    private static void ApplyLinkMarkToContent(object content, string href)
    {
        if (content is IDictionary<string, object> contentDict)
        {
            if (!contentDict.TryGetValue("marks", out var marksObj))
            {
                marksObj = new List<object>();
                contentDict["marks"] = marksObj;
            }
            var marks = (List<object>)marksObj;
            marks.Add(new Dictionary<string, object>
            {
                { "type", "link" },
                { "attrs", new Dictionary<string, object> { { "href", href } } }
            });
        }
        else if (content is List<object> contentList)
        {
            foreach (var item in contentList)
                ApplyLinkMarkToContent(item, href);
        }
    }

    private static void AddContentToList(List<object> contentList, object content)
    {
        if (content is List<object> list)
            contentList.AddRange(list);
        else if (content != null)
            contentList.Add(content);
    }
}