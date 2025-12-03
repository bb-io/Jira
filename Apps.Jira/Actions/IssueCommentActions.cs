using Apps.Jira.Dtos;
using Apps.Jira.Models.Identifiers;
using Apps.Jira.Models.Requests;
using Apps.Jira.Models.Responses;
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
    public async Task<IssueCommentDto[]> GetIssueComments([ActionParameter] GetIssueCommentsRequest input)
    {
        if (input.Issues != null)
        {
            var request = new JiraRequest($"/comment/list", Method.Post)
                .AddJsonBody(new
                {
                    ids = input.Issues.Select(int.Parse).ToList()
                });
            
            var comments = await Client.ExecuteWithHandling<ModelWrapper<List<IssueCommentDto>>>(request);
            return comments.Values.ToArray();
        }
        else
        {
            var request = new JiraRequest($"/issue/{input.IssueKey}/comment", Method.Get);
            var comments = await Client.ExecuteWithHandling<IssueCommentsWrapper>(request);
            return comments.Comments;
        }
    }

    [Action("Find issue comment by text", Description = "Find the first comment in an issue that contains the specified text.")]
    public async Task<CommentResponse> FindComment([ActionParameter] FindCommentRequest input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment", Method.Get);
        var commentsWrapper = await Client.ExecuteWithHandling<IssueCommentsWrapper>(request);

        if (commentsWrapper?.Comments == null || commentsWrapper.Comments.Length == 0)
            return null;

        foreach (var comment in commentsWrapper.Comments)
        {
            var bodyText = ExtractCommentText(comment);

            if (!string.IsNullOrEmpty(bodyText) &&
                bodyText.Contains(input.CommentContains, StringComparison.OrdinalIgnoreCase))
            {
                return new CommentResponse 
                {
                    CommentDto = comment,
                    CommentFlattenedText = bodyText
                    
                };
            }
        }

        return null;
    }


    [Action("Get issue comment", Description = "Get a comment of the specified issue.")]
    public async Task<IssueCommentDto> GetIssueComment([ActionParameter] IssueCommentIdentifier input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment/{input.CommentId}", Method.Get);
        return await Client.ExecuteWithHandling<IssueCommentDto>(request);
    }
    
    [Action("Delete issue comment", Description = "Delete a comment of the specified issue.")]
    public async Task DeleteIssueComment([ActionParameter] IssueCommentIdentifier input)
    {
        var request = new JiraRequest($"/issue/{input.IssueKey}/comment/{input.CommentId}", Method.Delete);
        await Client.ExecuteWithHandling(request);
    }
    
    [Action("Add issue comment", Description = "Add a comment to the specified issue.")]
    public async Task<IssueCommentDto> AddIssueComment([ActionParameter] IssueIdentifier input, 
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

        return await Client.ExecuteWithHandling<IssueCommentDto>(request);
    }
    
    [Action("Update issue comment", Description = "Update a comment of the specified issue.")]
    public async Task<IssueCommentDto> UpdateIssueComment([ActionParameter] IssueCommentIdentifier input, 
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
                        type = comment.Type ?? "doc",
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
        
        return await Client.ExecuteWithHandling<IssueCommentDto>(request);
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

