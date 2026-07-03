using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using UserDto = Apps.Jira.Dtos.UserDto;

namespace Apps.Jira.Actions;

[ActionList("Comments")]
public class IssueCommentActions : JiraInvocable
{
    public IssueCommentActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Get issue comments", Description = "Get comments of the specified issue.")]
    public async Task<GetIssueCommentsResponse> GetIssueComments([ActionParameter] GetIssueCommentsRequest input)
    {
        CommentWithTextResponse[] result;

        if (input.Issues != null)
        {
            var request = new JiraRequest($"/comment/list", Method.Post)
                .AddJsonBody(new
                {
                    ids = input.Issues.Select(int.Parse).ToList()
                });

            var comments = await Client
                .ExecuteWithHandling<ModelWrapper<List<IssueCommentDto>>>(request);

            result = comments.Values
                .Select(c => new CommentWithTextResponse
                {
                    Comment = c,
                    PlainText = c.ToPlainText()
                })
                .ToArray();
        }
        else
        {
            var request = new JiraRequest($"/issue/{input.IssueKey}/comment", Method.Get);
            var commentsWrapper = await Client.ExecuteWithHandling<IssueCommentsWrapper>(request);

            result = commentsWrapper.Comments?
                .Select(c => new CommentWithTextResponse
                {
                    Comment = c,
                    PlainText = c.ToPlainText()
                })
                .ToArray() ?? Array.Empty<CommentWithTextResponse>();
        }

        return new GetIssueCommentsResponse
        {
            Comments = result
        };
    }

    [Action("Find issue comment by text", Description = "Find the first comment in an issue that contains the specified text.")]
    public async Task<CommentWithTextResponse?> FindComment([ActionParameter] FindCommentRequest input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment", Method.Get);
        var commentsWrapper = await Client.ExecuteWithHandling<IssueCommentsWrapper>(request);

        if (commentsWrapper?.Comments == null || commentsWrapper.Comments.Length == 0)
            return null;

        static DateTime ParseJiraDateOrMin(string? value)
            => DateTime.TryParse(value, out var dt) ? dt : DateTime.MinValue;

        var matches = commentsWrapper.Comments
            .Select(c => new
            {
                Comment = c,
                PlainText = c.ToPlainText(),
                Updated = ParseJiraDateOrMin(c.Updated),
                Created = ParseJiraDateOrMin(c.Created)
            })
            .Where(x => !string.IsNullOrEmpty(x.PlainText) &&
                        x.PlainText.Contains(input.CommentContains, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
            return null;

        var selected = input.Latest == true
            ? matches.OrderByDescending(x => x.Updated != DateTime.MinValue ? x.Updated : x.Created).First()
            : matches.First();

        return new CommentWithTextResponse
        {
            Comment = selected.Comment,
            PlainText = selected.PlainText
        };
    }


    [Action("Get issue comment", Description = "Get a comment of the specified issue.")]
    public async Task<CommentWithTextResponse> GetIssueComment([ActionParameter] IssueCommentIdentifier input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment/{input.CommentId}", Method.Get);
        var comment = await Client.ExecuteWithHandling<IssueCommentDto>(request);

        return new CommentWithTextResponse
        {
            Comment = comment,
            PlainText = comment.ToPlainText()
        };
    }

    [Action("Delete issue comment", Description = "Delete a comment of the specified issue.")]
    public async Task DeleteIssueComment([ActionParameter] IssueCommentIdentifier input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment/{input.CommentId}", Method.Delete);
        await Client.ExecuteWithHandling(request);
    }

    [Action("Add issue comment", Description = "Add a comment to the specified issue.")]
    public async Task<CommentWithTextResponse> AddIssueComment(
    [ActionParameter] IssueIdentifier input,
    [ActionParameter] AddIssueCommentRequest comment)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment", Method.Post);

        var payload = new
        {
            body = await BuildCommentBody(comment)
        };

        request.AddStringBody(
            JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }),
            DataFormat.Json);

        var created = await Client.ExecuteWithHandling<IssueCommentDto>(request);

        return new CommentWithTextResponse
        {
            Comment = created,
            PlainText = created.ToPlainText()
        };
    }

    private async Task<Dictionary<string, string>> GetDisplayNamesByAccountIds(List<string> accountIds)
    {
        var result = new Dictionary<string, string>();

        foreach (var id in accountIds)
        {
            try
            {
                var userReq = new JiraRequest("/user", Method.Get);
                userReq.AddQueryParameter("accountId", id);

                var user = await Client.ExecuteWithHandling<UserDto>(userReq);

                if (!string.IsNullOrWhiteSpace(user?.DisplayName))
                    result[id] = user.DisplayName;
            }
            catch
            {
            }
        }

        return result;
    }

    [Action("Append text to comment", Description = "Append text to comment of the specified issue.")]
    public async Task<CommentWithTextResponse> UpdateIssueComment(
        [ActionParameter] IssueCommentIdentifier input,
        [ActionParameter] AddIssueCommentRequest comment)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment/{input.CommentId}", Method.Put);

        request.AddJsonBody(new
        {
            body = await BuildCommentBody(comment)
        });

        var updated = await Client.ExecuteWithHandling<IssueCommentDto>(request);

        return new CommentWithTextResponse
        {
            Comment = updated,
            PlainText = updated.ToPlainText()
        };
    }

    private async Task<Dictionary<string, object>> BuildCommentBody(AddIssueCommentRequest comment)
    {
        var paragraphs = new List<Dictionary<string, object>>();

        if (!string.IsNullOrWhiteSpace(comment.LinkText) && string.IsNullOrWhiteSpace(comment.LinkUrl))
            throw new PluginMisconfigurationException("Link URL must be provided when link text is specified.");

        if (!string.IsNullOrWhiteSpace(comment.Text))
            paragraphs.Add(CreateParagraph([CreateTextNode(comment.Text)]));

        if (!string.IsNullOrWhiteSpace(comment.LinkUrl))
        {
            var linkUrl = comment.LinkUrl.Trim();
            var linkText = string.IsNullOrWhiteSpace(comment.LinkText) ? linkUrl : comment.LinkText;

            paragraphs.Add(CreateParagraph([
                CreateTextNode(linkText, [CreateLinkMark(linkUrl)])
            ]));
        }

        var ids = (comment.MentionAccountIds ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct()
            .ToList();

        if (ids.Count > 0)
        {
            var namesById = await GetDisplayNamesByAccountIds(ids);
            var mentionContent = new List<Dictionary<string, object>>();

            for (var i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                var displayName = namesById.TryGetValue(id, out var dn) && !string.IsNullOrWhiteSpace(dn)
                    ? dn
                    : "user";

                mentionContent.Add(CreateMentionNode(id, displayName));

                if (i < ids.Count - 1)
                    mentionContent.Add(CreateTextNode(", "));
            }

            paragraphs.Add(CreateParagraph(mentionContent));
        }

        if (paragraphs.Count == 0)
            throw new PluginMisconfigurationException("Comment must contain text, a link, or at least one mentioned user.");

        return new Dictionary<string, object>
        {
            ["type"] = "doc",
            ["version"] = 1,
            ["content"] = paragraphs
        };
    }

    private static Dictionary<string, object> CreateParagraph(List<Dictionary<string, object>> content)
    {
        return new Dictionary<string, object>
        {
            ["type"] = "paragraph",
            ["content"] = content
        };
    }

    private static Dictionary<string, object> CreateTextNode(string text, List<Dictionary<string, object>>? marks = null)
    {
        var textNode = new Dictionary<string, object>
        {
            ["type"] = "text",
            ["text"] = text
        };

        if (marks?.Any() == true)
            textNode["marks"] = marks;

        return textNode;
    }

    private static Dictionary<string, object> CreateLinkMark(string url)
    {
        return new Dictionary<string, object>
        {
            ["type"] = "link",
            ["attrs"] = new Dictionary<string, object>
            {
                ["href"] = url,
                ["title"] = url
            }
        };
    }

    private static Dictionary<string, object> CreateMentionNode(string id, string displayName)
    {
        return new Dictionary<string, object>
        {
            ["type"] = "mention",
            ["attrs"] = new Dictionary<string, object>
            {
                ["id"] = id,
                ["text"] = "@" + displayName,
                ["accessLevel"] = "NONE"
            }
        };
    }
}

