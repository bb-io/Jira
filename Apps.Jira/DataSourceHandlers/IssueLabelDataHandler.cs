using Apps.Jira.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Jira.Dtos;
using RestSharp;

namespace Apps.Jira.DataSourceHandlers
{
    public class IssueLabelDataHandler(InvocationContext invocationContext) : JiraInvocable(invocationContext), IAsyncDataSourceHandler
    {
        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            const int maxResultsPerPage = 1000;
            var startAt = 0;
            var isLast = false;

            var allLabels = new List<string>();
            while (!isLast)
            {
                var request = new JiraRequest($"/label?startAt={startAt}&maxResults={maxResultsPerPage}", Method.Get);
                var labels = await Client.ExecuteWithHandling<LabelsPaginationDto>(request);

                if (labels?.Values != null)
                {
                    allLabels.AddRange(labels.Values);
                }

                startAt += labels?.MaxResults ?? maxResultsPerPage;
                isLast = labels?.IsLast ?? true;
            }

            return allLabels
                .Where(x => context.SearchString == null
                            || x.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(l => l, l => l);
        }
    }
}
