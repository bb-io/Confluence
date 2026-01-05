using Blackbird.Applications.Sdk.Common;

namespace Apps.Confluence.Models.Identifiers
{
    public class PropertyIdentifier : ContentIdentifier
    {
        [Display("Property key")]
        public string Key { get; set; }
    }
}
