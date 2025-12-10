using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
using Apps.Jira.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;
using System.Text;

namespace Apps.Jira.Actions;

[ActionList]
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

        foreach (var comment in commentsWrapper.Comments)
        {
            var bodyText = comment.ToPlainText();

            if (!string.IsNullOrEmpty(bodyText) &&
                bodyText.Contains(input.CommentContains, StringComparison.OrdinalIgnoreCase))
            {
                return new CommentWithTextResponse
                {
                    Comment = comment,
                    PlainText = bodyText
                };
            }
        }

        return null;
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

        request.AddStringBody(JsonConvert.SerializeObject(new
        {
            body = new
            {
                type = comment.BodyType ?? "doc",
                version = comment.Version == null ? 1 : int.Parse(comment.Version),
                content = new[]
                {
                new
                {
                    type = comment.Type ?? "paragraph",
                    content = new[]
                    {
                        new
                        {
                            type = comment.ContentType ?? "text",
                            text = comment.Text
                        }
                    }
                }
            }
            },
        }, Formatting.None,
        new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        }), DataFormat.Json);

        var created = await Client.ExecuteWithHandling<IssueCommentDto>(request);

        return new CommentWithTextResponse
        {
            Comment = created,
            PlainText = created.ToPlainText()
        };
    }

    [Action("Append text to comment", Description = "Append text to comment of the specified issue.")]
public async Task<CommentWithTextResponse> UpdateIssueComment(
    [ActionParameter] IssueCommentIdentifier input,
    [ActionParameter] AddIssueCommentRequest comment)
{
    var request = new JiraRequest($"/issue/{input.IssueKey}/comment/{input.CommentId}", Method.Put);

    request.AddJsonBody(new
    {
        body = new
        {
            content = new[]
            {
                new
                {
                    type = comment.Type ?? "paragraph",
                    content = new[]
                    {
                        new
                        {
                            type = comment.ContentType ?? "text",
                            text = comment.Text
                        }
                    }
                }
            },
            type = comment.BodyType ?? "doc",
            version = comment.Version == null ? 1 : int.Parse(comment.Version)
        },
    });

    var updated = await Client.ExecuteWithHandling<IssueCommentDto>(request);

    return new CommentWithTextResponse
    {
        Comment = updated,
        PlainText = updated.ToPlainText()
    };
}


    private string ExtractCommentText(IssueCommentDto comment)
    {
        if (comment.Body?.Content == null)
            return string.Empty;

        var sb = new StringBuilder();

        foreach (var block in comment.Body.Content) 
        {
            if (block?.Content == null)
                continue;

            foreach (var part in block.Content) 
            {
                if (!string.IsNullOrEmpty(part?.Text))
                {
                    sb.Append(part.Text);
                    sb.Append(" ");
                }
            }
        }

        return sb.ToString().Trim();
    }
}

