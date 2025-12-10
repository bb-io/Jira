using Apps.Jira.Dtos;
using System.Text;

namespace Apps.Jira.Utils
{
    public static class CommentTextHelper
    {
        public static string ExtractCommentText(IssueCommentDto comment)
        {
            if (comment?.Body?.Content == null)
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
                        sb.Append(' ');
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }

    public static class CommentExtensions
    {
        public static string ToPlainText(this IssueCommentDto comment)
            => CommentTextHelper.ExtractCommentText(comment);
    }
}
