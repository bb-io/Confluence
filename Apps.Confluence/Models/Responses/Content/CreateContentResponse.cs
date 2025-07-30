using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Confluence.Models.Responses.Content
{
    public class CreateContentResponse
    {
        [Display("Content ID")]
        public string Id { get; set; } = string.Empty;

        [Display("Status")]
        public string Status { get; set; } = string.Empty;

        [Display("Title")]
        public string Title { get; set; } = string.Empty;

        [Display("Space ID")]
        public string SpaceId { get; set; } = string.Empty;

        [Display("Parent ID")]
        public string ParentId { get; set; } = null;

        [Display("Owner ID")]
        public string OwnerId { get; set; } = null;

        [Display("Last owner ID")]
        public string LastOwnerId { get; set; } = null;

        [Display("Parent type")]
        public string ParentType { get; set; } = null;

        [Display("Position")]
        public int? Position { get; set; }

        [Display("Author ID")]
        public string AuthorId { get; set; } = string.Empty;

        [Display("Created at")]
        public DateTime CreatedAt { get; set; }

        [Display("Body")]
        public BodyResponse Body { get; set; } = new();

        [DefinitionIgnore]
        public VersionResponse Version { get; set; } = new();

        [JsonProperty("_links")]
        [DefinitionIgnore]
        public LinksResponse Links { get; set; } = new();
    }

    public class StorageResponse
    {
        [Display("Representation")]
        public string Representation { get; set; } = string.Empty;

        [Display("Value")]
        public string Value { get; set; } = string.Empty;
    }

    public class LinksResponse
    {
        [Display("Edit UI")]
        public string Editui { get; set; } = string.Empty;

        [Display("Web UI")]
        public string Webui { get; set; } = string.Empty;

        [Display("Edit UI v2")]
        public string Edituiv2 { get; set; } = string.Empty;

        [Display("Tiny UI")]
        public string Tinyui { get; set; } = string.Empty;

        [Display("Base")]
        public string Base { get; set; } = string.Empty;
    }
}