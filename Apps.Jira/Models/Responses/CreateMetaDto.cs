using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Jira.Models.Responses
{
    public class CreateMetaDto
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("projects")]
        public IEnumerable<ProjectMetaDto> Projects { get; set; }
    }

    public class ProjectMetaDto
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatarUrls")]
        public AvatarUrlsDto AvatarUrls { get; set; }

        [JsonProperty("issuetypes")]
        public IEnumerable<IssueTypeMetaDto> IssueTypes { get; set; }
    }

    public class AvatarUrlsDto
    {
        [JsonProperty("48x48")]
        public string X48 { get; set; }

        [JsonProperty("24x24")]
        public string X24 { get; set; }

        [JsonProperty("16x16")]
        public string X16 { get; set; }

        [JsonProperty("32x32")]
        public string X32 { get; set; }
    }

    public class IssueTypeMetaDto
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("untranslatedName")]
        public string UntranslatedName { get; set; }

        [JsonProperty("subtask")]
        public bool Subtask { get; set; }

        [JsonProperty("hierarchyLevel")]
        public int HierarchyLevel { get; set; }

        [JsonProperty("scope")]
        public ScopeDto Scope { get; set; }

        [JsonProperty("expand")]
        public string Expand { get; set; }
    }

    public class ScopeDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("project")]
        public ProjectScopeDto Project { get; set; }
    }

    public class ProjectScopeDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
